using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public class ModalCommandInfo : CommandInfo<ModalCommandParameterInfo>
{
	public ModalInfo Modal { get; }

	public override bool SupportsWildCards => true;

	public override IReadOnlyList<ModalCommandParameterInfo> Parameters { get; }

	internal ModalCommandInfo(ModalCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		Parameters = builder.Parameters.Select((ModalCommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
		Modal = Parameters.Last().Modal;
	}

	public override Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is IModalInteraction))
		{
			return Task.FromResult((IResult)ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Modal Interaction."));
		}
		return base.ExecuteAsync(context, services);
	}

	protected override async Task<IResult> ParseArgumentsAsync(IInteractionContext context, IServiceProvider services)
	{
		List<IRouteSegmentMatch> captures = (context as IRouteMatchContainer)?.SegmentMatches?.ToList();
		int captureCount = captures?.Count() ?? 0;
		try
		{
			object[] args = new object[Parameters.Count];
			for (int i = 0; i < Parameters.Count; i++)
			{
				ModalCommandParameterInfo modalCommandParameterInfo = Parameters.ElementAt(i);
				if (i < captureCount)
				{
					TypeConverterResult result = await modalCommandParameterInfo.TypeReader.ReadAsync(context, captures[i].Value, services).ConfigureAwait(continueOnCapturedContext: false);
					if (!result.IsSuccess)
					{
						return await InvokeEventAndReturn(context, result).ConfigureAwait(continueOnCapturedContext: false);
					}
					args[i] = result.Value;
					continue;
				}
				IResult result2 = await Modal.CreateModalAsync(context, services, base.Module.CommandService._exitOnMissingModalField).ConfigureAwait(continueOnCapturedContext: false);
				if (!result2.IsSuccess)
				{
					return await InvokeEventAndReturn(context, result2).ConfigureAwait(continueOnCapturedContext: false);
				}
				if (!(result2 is TypeConverterResult typeConverterResult))
				{
					return await InvokeEventAndReturn(context, ExecuteResult.FromError(InteractionCommandError.BadArgs, "Command parameter parsing failed for an unknown reason."));
				}
				args[i] = typeConverterResult.Value;
			}
			return ParseResult.FromSuccess(args);
		}
		catch (Exception exception)
		{
			return await InvokeEventAndReturn(context, ExecuteResult.FromError(exception)).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	protected override Task InvokeModuleEvent(IInteractionContext context, IResult result)
	{
		return base.CommandService._modalCommandExecutedEvent.InvokeAsync(this, context, result);
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Modal Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Modal Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}
}

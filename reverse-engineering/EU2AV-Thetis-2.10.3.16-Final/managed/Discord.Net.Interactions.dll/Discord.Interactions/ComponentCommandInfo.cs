using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public class ComponentCommandInfo : CommandInfo<ComponentCommandParameterInfo>
{
	public override IReadOnlyList<ComponentCommandParameterInfo> Parameters { get; }

	public override bool SupportsWildCards => true;

	internal ComponentCommandInfo(ComponentCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		Parameters = builder.Parameters.Select((ComponentCommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
	}

	public override Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is IComponentInteraction))
		{
			return Task.FromResult((IResult)ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Message Component Interaction"));
		}
		return base.ExecuteAsync(context, services);
	}

	protected override async Task<IResult> ParseArgumentsAsync(IInteractionContext context, IServiceProvider services)
	{
		List<IRouteSegmentMatch> captures = (context as IRouteMatchContainer)?.SegmentMatches?.ToList();
		int captureCount = captures?.Count() ?? 0;
		try
		{
			IComponentInteractionData data = (context.Interaction as IComponentInteraction).Data;
			object[] args = new object[Parameters.Count];
			for (int i = 0; i < Parameters.Count; i++)
			{
				ComponentCommandParameterInfo componentCommandParameterInfo = Parameters[i];
				bool flag = i < captureCount;
				if (flag ^ componentCommandParameterInfo.IsRouteSegmentParameter)
				{
					return await InvokeEventAndReturn(context, ExecuteResult.FromError(InteractionCommandError.BadArgs, "Argument type and parameter type didn't match (Wild Card capture/Component value)")).ConfigureAwait(continueOnCapturedContext: false);
				}
				TypeConverterResult typeConverterResult = ((!flag) ? (await componentCommandParameterInfo.TypeConverter.ReadAsync(context, data, services).ConfigureAwait(continueOnCapturedContext: false)) : (await componentCommandParameterInfo.TypeReader.ReadAsync(context, captures[i].Value, services).ConfigureAwait(continueOnCapturedContext: false)));
				TypeConverterResult result = typeConverterResult;
				if (!result.IsSuccess)
				{
					return await InvokeEventAndReturn(context, result).ConfigureAwait(continueOnCapturedContext: false);
				}
				args[i] = result.Value;
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
		return base.CommandService._componentCommandExecutedEvent.InvokeAsync(this, context, result);
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Component Interaction: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Component Interaction: \"{ToString()}\" for {context.User} in {context.Channel}";
	}
}

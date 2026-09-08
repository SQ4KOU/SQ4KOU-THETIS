using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public class SlashCommandInfo : CommandInfo<SlashCommandParameterInfo>, IApplicationCommandInfo
{
	internal IReadOnlyDictionary<string, SlashCommandParameterInfo> _flattenedParameterDictionary { get; }

	public string Description { get; }

	public ApplicationCommandType CommandType { get; } = ApplicationCommandType.Slash;

	public bool DefaultPermission { get; }

	public bool IsEnabledInDm { get; }

	public bool IsNsfw { get; }

	public GuildPermission? DefaultMemberPermissions { get; }

	public override IReadOnlyList<SlashCommandParameterInfo> Parameters { get; }

	public override bool SupportsWildCards => false;

	public IReadOnlyList<SlashCommandParameterInfo> FlattenedParameters { get; }

	public IReadOnlyCollection<InteractionContextType> ContextTypes { get; }

	public IReadOnlyCollection<ApplicationIntegrationType> IntegrationTypes { get; }

	internal SlashCommandInfo(Discord.Interactions.Builders.SlashCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		Description = builder.Description;
		DefaultPermission = builder.DefaultPermission;
		IsEnabledInDm = builder.IsEnabledInDm;
		IsNsfw = builder.IsNsfw;
		DefaultMemberPermissions = builder.DefaultMemberPermissions;
		Parameters = builder.Parameters.Select((SlashCommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
		FlattenedParameters = FlattenParameters(Parameters).ToImmutableArray();
		ContextTypes = builder.ContextTypes?.ToImmutableArray();
		IntegrationTypes = builder.IntegrationTypes?.ToImmutableArray();
		for (int num = 0; num < FlattenedParameters.Count - 1; num++)
		{
			if (!FlattenedParameters.ElementAt(num).IsRequired && FlattenedParameters.ElementAt(num + 1).IsRequired)
			{
				throw new InvalidOperationException("Optional parameters must appear after all required parameters, ComplexParameters with optional parameters must be located at the end.");
			}
		}
		_flattenedParameterDictionary = FlattenedParameters?.ToDictionary((SlashCommandParameterInfo x) => x.Name, (SlashCommandParameterInfo x) => x).ToImmutableDictionary();
	}

	public override Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is ISlashCommandInteraction))
		{
			return Task.FromResult((IResult)ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Slash Command Interaction"));
		}
		return base.ExecuteAsync(context, services);
	}

	protected override async Task<IResult> ParseArgumentsAsync(IInteractionContext context, IServiceProvider services)
	{
		List<IApplicationCommandInteractionDataOption> options = GetOptions();
		object[] args = new object[Parameters.Count];
		for (int i = 0; i < Parameters.Count; i++)
		{
			SlashCommandParameterInfo parameterInfo = Parameters[i];
			IResult result = await ParseArgumentAsync(parameterInfo, context, options, services).ConfigureAwait(continueOnCapturedContext: false);
			if (!result.IsSuccess)
			{
				return ParseResult.FromError(result);
			}
			if (!(result is TypeConverterResult typeConverterResult))
			{
				return ExecuteResult.FromError(InteractionCommandError.BadArgs, "Complex command parsing failed for an unknown reason.");
			}
			args[i] = typeConverterResult.Value;
		}
		return ParseResult.FromSuccess(args);
		List<IApplicationCommandInteractionDataOption> GetOptions()
		{
			IReadOnlyCollection<IApplicationCommandInteractionDataOption> readOnlyCollection = (context.Interaction as ISlashCommandInteraction).Data.Options;
			while (readOnlyCollection != null && readOnlyCollection.Any((IApplicationCommandInteractionDataOption x) => x.Type == ApplicationCommandOptionType.SubCommand || x.Type == ApplicationCommandOptionType.SubCommandGroup))
			{
				readOnlyCollection = readOnlyCollection.ElementAt(0)?.Options;
			}
			return readOnlyCollection.ToList();
		}
	}

	private async ValueTask<IResult> ParseArgumentAsync(SlashCommandParameterInfo parameterInfo, IInteractionContext context, List<IApplicationCommandInteractionDataOption> argList, IServiceProvider services)
	{
		if (parameterInfo.IsComplexParameter)
		{
			object[] ctorArgs = new object[parameterInfo.ComplexParameterFields.Count];
			for (int i = 0; i < ctorArgs.Length; i++)
			{
				IResult result = await ParseArgumentAsync(parameterInfo.ComplexParameterFields.ElementAt(i), context, argList, services).ConfigureAwait(continueOnCapturedContext: false);
				if (!result.IsSuccess)
				{
					return result;
				}
				if (!(result is TypeConverterResult typeConverterResult))
				{
					return ExecuteResult.FromError(InteractionCommandError.BadArgs, "Complex command parsing failed for an unknown reason.");
				}
				ctorArgs[i] = typeConverterResult.Value;
			}
			return TypeConverterResult.FromSuccess(parameterInfo._complexParameterInitializer(ctorArgs));
		}
		IApplicationCommandInteractionDataOption applicationCommandInteractionDataOption = argList?.Find((IApplicationCommandInteractionDataOption x) => string.Equals(x.Name, parameterInfo.Name, StringComparison.OrdinalIgnoreCase));
		if (applicationCommandInteractionDataOption == null)
		{
			IResult result3;
			if (!parameterInfo.IsRequired)
			{
				IResult result2 = TypeConverterResult.FromSuccess(parameterInfo.DefaultValue);
				result3 = result2;
			}
			else
			{
				IResult result2 = ExecuteResult.FromError(InteractionCommandError.BadArgs, "Command was invoked with too few parameters");
				result3 = result2;
			}
			return result3;
		}
		return await parameterInfo.TypeConverter.ReadAsync(context, applicationCommandInteractionDataOption, services).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected override Task InvokeModuleEvent(IInteractionContext context, IResult result)
	{
		return base.CommandService._slashCommandExecutedEvent.InvokeAsync(this, context, result);
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Slash Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Slash Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}

	private static IEnumerable<SlashCommandParameterInfo> FlattenParameters(IEnumerable<SlashCommandParameterInfo> parameters)
	{
		foreach (SlashCommandParameterInfo parameter in parameters)
		{
			if (!parameter.IsComplexParameter)
			{
				yield return parameter;
				continue;
			}
			foreach (SlashCommandParameterInfo complexParameterField in parameter.ComplexParameterFields)
			{
				yield return complexParameterField;
			}
		}
	}
}

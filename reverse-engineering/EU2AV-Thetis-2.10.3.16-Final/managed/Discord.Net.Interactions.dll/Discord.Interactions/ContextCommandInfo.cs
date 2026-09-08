using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public abstract class ContextCommandInfo : CommandInfo<CommandParameterInfo>, IApplicationCommandInfo
{
	public ApplicationCommandType CommandType { get; }

	public bool DefaultPermission { get; }

	public bool IsEnabledInDm { get; }

	public bool IsNsfw { get; }

	public GuildPermission? DefaultMemberPermissions { get; }

	public IReadOnlyCollection<InteractionContextType> ContextTypes { get; }

	public IReadOnlyCollection<ApplicationIntegrationType> IntegrationTypes { get; }

	public override IReadOnlyList<CommandParameterInfo> Parameters { get; }

	public override bool SupportsWildCards => false;

	public override bool IgnoreGroupNames => true;

	internal ContextCommandInfo(ContextCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base((ICommandBuilder)builder, module, commandService)
	{
		CommandType = builder.CommandType;
		DefaultPermission = builder.DefaultPermission;
		IsNsfw = builder.IsNsfw;
		IsEnabledInDm = builder.IsEnabledInDm;
		DefaultMemberPermissions = builder.DefaultMemberPermissions;
		Parameters = builder.Parameters.Select((CommandParameterBuilder x) => x.Build(this)).ToImmutableArray();
		ContextTypes = builder.ContextTypes?.ToImmutableArray();
		IntegrationTypes = builder.IntegrationTypes?.ToImmutableArray();
	}

	internal static ContextCommandInfo Create(ContextCommandBuilder builder, ModuleInfo module, InteractionService commandService)
	{
		return builder.CommandType switch
		{
			ApplicationCommandType.User => new UserCommandInfo(builder, module, commandService), 
			ApplicationCommandType.Message => new MessageCommandInfo(builder, module, commandService), 
			_ => throw new InvalidOperationException("This command type is not a supported Context Command"), 
		};
	}

	protected override Task InvokeModuleEvent(IInteractionContext context, IResult result)
	{
		return base.CommandService._contextCommandExecutedEvent.InvokeAsync(this, context, result);
	}
}

using System;
using System.Collections.Generic;

namespace Discord.Interactions;

public interface IApplicationCommandInfo
{
	string Name { get; }

	ApplicationCommandType CommandType { get; }

	[Obsolete("To be deprecated soon, use IsEnabledInDm and DefaultMemberPermissions instead.")]
	bool DefaultPermission { get; }

	bool IsEnabledInDm { get; }

	bool IsNsfw { get; }

	GuildPermission? DefaultMemberPermissions { get; }

	IReadOnlyCollection<InteractionContextType> ContextTypes { get; }

	IReadOnlyCollection<ApplicationIntegrationType> IntegrationTypes { get; }
}

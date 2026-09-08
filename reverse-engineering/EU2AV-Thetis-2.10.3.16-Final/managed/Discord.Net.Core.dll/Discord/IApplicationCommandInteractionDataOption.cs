using System.Collections.Generic;

namespace Discord;

public interface IApplicationCommandInteractionDataOption
{
	string Name { get; }

	object Value { get; }

	ApplicationCommandOptionType Type { get; }

	IReadOnlyCollection<IApplicationCommandInteractionDataOption> Options { get; }
}

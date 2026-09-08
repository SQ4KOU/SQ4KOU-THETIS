using System.Collections.Generic;

namespace Discord;

public interface IApplicationCommandOptionChoice
{
	string Name { get; }

	object Value { get; }

	IReadOnlyDictionary<string, string> NameLocalizations { get; }

	string NameLocalized { get; }
}

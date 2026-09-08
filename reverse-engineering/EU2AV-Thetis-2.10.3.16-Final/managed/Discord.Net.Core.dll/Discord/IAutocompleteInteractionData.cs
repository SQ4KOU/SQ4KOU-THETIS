using System.Collections.Generic;

namespace Discord;

public interface IAutocompleteInteractionData : IDiscordInteractionData
{
	string CommandName { get; }

	ulong CommandId { get; }

	ApplicationCommandType Type { get; }

	ulong Version { get; }

	AutocompleteOption Current { get; }

	IReadOnlyCollection<AutocompleteOption> Options { get; }
}

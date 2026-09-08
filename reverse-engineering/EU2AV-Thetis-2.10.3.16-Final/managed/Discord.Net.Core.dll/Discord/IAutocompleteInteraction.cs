using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface IAutocompleteInteraction : IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IAutocompleteInteractionData Data { get; }

	Task RespondAsync(IEnumerable<AutocompleteResult> result, RequestOptions options = null);
}

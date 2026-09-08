using System.Collections.Generic;

namespace Discord;

public interface IModalInteractionData : IDiscordInteractionData
{
	string CustomId { get; }

	IReadOnlyCollection<IComponentInteractionData> Components { get; }
}

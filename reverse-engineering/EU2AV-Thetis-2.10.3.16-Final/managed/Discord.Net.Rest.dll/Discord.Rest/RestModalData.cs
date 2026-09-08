using System.Collections.Generic;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class RestModalData : IModalInteractionData, IDiscordInteractionData
{
	public string CustomId { get; }

	public IReadOnlyCollection<RestMessageComponentData> Components { get; }

	IReadOnlyCollection<IComponentInteractionData> IModalInteractionData.Components => Components;

	internal RestModalData(ModalInteractionData model, BaseDiscordClient discord, IGuild guild)
	{
		CustomId = model.CustomId;
		Components = (from x in model.Components.SelectMany((Discord.API.ActionRowComponent x) => x.Components.OfType<IInteractableComponent>())
			select new RestMessageComponentData(x, discord, guild)).ToArray();
	}
}

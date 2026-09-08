using System.Collections.Generic;
using System.Linq;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketModalData : IModalInteractionData, IDiscordInteractionData
{
	public string CustomId { get; }

	public IReadOnlyCollection<SocketMessageComponentData> Components { get; }

	IReadOnlyCollection<IComponentInteractionData> IModalInteractionData.Components => Components;

	internal SocketModalData(ModalInteractionData model, DiscordSocketClient discord, ClientState state, SocketGuild guild, User dmUser)
	{
		CustomId = model.CustomId;
		Components = (from x in model.Components.SelectMany((Discord.API.ActionRowComponent x) => x.Components.Select((IMessageComponent y) => y.ToEntity()).OfType<IInteractableComponent>())
			select new SocketMessageComponentData(x, discord, state, guild, dmUser)).ToArray();
	}
}

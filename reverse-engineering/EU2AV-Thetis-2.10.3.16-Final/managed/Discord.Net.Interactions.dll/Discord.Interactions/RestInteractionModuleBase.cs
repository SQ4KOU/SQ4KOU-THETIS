using System;
using System.Threading.Tasks;
using Discord.Rest;

namespace Discord.Interactions;

public abstract class RestInteractionModuleBase<T> : InteractionModuleBase<T> where T : class, IInteractionContext
{
	public InteractionService InteractionService { get; set; }

	protected override Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
	{
		return HandleInteractionAsync((RestInteraction x) => x.Defer(ephemeral, options));
	}

	protected override Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return HandleInteractionAsync((RestInteraction x) => x.Respond(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags));
	}

	protected override Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		return HandleInteractionAsync((RestInteraction x) => x.RespondWithModal(modal, options));
	}

	protected override async Task RespondWithModalAsync<TModal>(string customId, TModal modal, RequestOptions options = null, Action<ModalBuilder> modifyModal = null)
	{
		await HandleInteractionAsync((RestInteraction x) => x.RespondWithModal(customId, modal, options, modifyModal));
	}

	protected override Task RespondWithModalAsync<TModal>(string customId, RequestOptions options = null, Action<ModalBuilder> modifyModal = null)
	{
		return HandleInteractionAsync((RestInteraction x) => x.RespondWithModal<TModal>(customId, options, modifyModal));
	}

	private Task HandleInteractionAsync(Func<RestInteraction, string> action)
	{
		if (!(base.Context.Interaction is RestInteraction arg))
		{
			throw new InvalidOperationException("Interaction must be a type of RestInteraction in order to execute this method.");
		}
		string text = action(arg);
		if (base.Context is IRestInteractionContext { InteractionResponseCallback: not null } restInteractionContext)
		{
			return restInteractionContext.InteractionResponseCallback(text);
		}
		return InteractionService._restResponseCallback(base.Context, text);
	}
}

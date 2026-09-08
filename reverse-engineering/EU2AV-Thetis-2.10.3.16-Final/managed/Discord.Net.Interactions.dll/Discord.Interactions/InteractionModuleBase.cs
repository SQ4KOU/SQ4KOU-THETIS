using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public abstract class InteractionModuleBase<T> : IInteractionModuleBase where T : class, IInteractionContext
{
	public T Context { get; private set; }

	public virtual void AfterExecute(ICommandInfo command)
	{
	}

	public virtual void BeforeExecute(ICommandInfo command)
	{
	}

	public virtual Task BeforeExecuteAsync(ICommandInfo command)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterExecuteAsync(ICommandInfo command)
	{
		return Task.CompletedTask;
	}

	public virtual void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
	{
	}

	public virtual void Construct(ModuleBuilder builder, InteractionService commandService)
	{
	}

	internal void SetContext(IInteractionContext context)
	{
		T val = context as T;
		Context = val ?? throw new InvalidOperationException("Invalid context type. Expected " + typeof(T).Name + ", got " + context.GetType().Name + ".");
	}

	protected virtual Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
	{
		return Context.Interaction.DeferAsync(ephemeral, options);
	}

	protected virtual Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task RespondWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.RespondWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task RespondWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.RespondWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task RespondWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.RespondWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task<IUserMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task<IUserMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.FollowupWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task<IUserMessage> FollowupWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.FollowupWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task<IUserMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.FollowupWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task<IUserMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return Context.Interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	protected virtual Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, PollProperties poll = null)
	{
		return Context.Channel.SendMessageAsync(text, isTTS: false, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags, poll);
	}

	protected virtual Task<IUserMessage> GetOriginalResponseAsync(RequestOptions options = null)
	{
		return Context.Interaction.GetOriginalResponseAsync(options);
	}

	protected virtual Task<IUserMessage> ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		return Context.Interaction.ModifyOriginalResponseAsync(func, options);
	}

	protected virtual Task DeleteOriginalResponseAsync()
	{
		return Context.Interaction.DeleteOriginalResponseAsync();
	}

	protected virtual Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		return Context.Interaction.RespondWithModalAsync(modal, options);
	}

	protected virtual Task RespondWithModalAsync<TModal>(string customId, TModal modal, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where TModal : class, IModal
	{
		return Context.Interaction.RespondWithModalAsync(customId, modal, options, modifyModal);
	}

	protected virtual Task RespondWithModalAsync<TModal>(string customId, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where TModal : class, IModal
	{
		return Context.Interaction.RespondWithModalAsync<TModal>(customId, options, modifyModal);
	}

	protected virtual Task RespondWithPremiumRequiredAsync(RequestOptions options = null)
	{
		return Context.Interaction.RespondWithPremiumRequiredAsync(options);
	}

	void IInteractionModuleBase.SetContext(IInteractionContext context)
	{
		SetContext(context);
	}
}
public abstract class InteractionModuleBase : InteractionModuleBase<IInteractionContext>
{
}

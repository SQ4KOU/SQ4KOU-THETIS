using System;
using System.Collections.Generic;
using Discord.API.Gateway;

namespace Discord.WebSocket;

public class SocketReaction : IReaction
{
	public ulong UserId { get; }

	public Optional<IUser> User { get; }

	public ulong MessageId { get; }

	public Optional<SocketUserMessage> Message { get; }

	public ISocketMessageChannel Channel { get; }

	public IEmote Emote { get; }

	public bool IsBurst { get; }

	public IReadOnlyCollection<Color> BurstColors { get; }

	public ReactionType ReactionType { get; }

	internal SocketReaction(ISocketMessageChannel channel, ulong messageId, Optional<SocketUserMessage> message, ulong userId, Optional<IUser> user, IEmote emoji, bool isBurst, IReadOnlyCollection<Color> colors, ReactionType reactionType)
	{
		Channel = channel;
		MessageId = messageId;
		Message = message;
		UserId = userId;
		User = user;
		Emote = emoji;
		IsBurst = isBurst;
		BurstColors = colors;
		ReactionType = reactionType;
	}

	internal static SocketReaction Create(Reaction model, ISocketMessageChannel channel, Optional<SocketUserMessage> message, Optional<IUser> user)
	{
		return new SocketReaction(emoji: (!model.Emoji.Id.HasValue) ? ((IEmote)new Emoji(model.Emoji.Name)) : ((IEmote)new Emote(model.Emoji.Id.Value, model.Emoji.Name, model.Emoji.Animated.GetValueOrDefault())), channel: channel, messageId: model.MessageId, message: message, userId: model.UserId, user: user, isBurst: model.IsBurst, colors: model.BurstColors.GetValueOrDefault(Array.Empty<Color>()).ToReadOnlyCollection(), reactionType: model.Type);
	}

	public override bool Equals(object other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!(other is SocketReaction socketReaction))
		{
			return false;
		}
		if (UserId == socketReaction.UserId && MessageId == socketReaction.MessageId)
		{
			return Emote.Equals(socketReaction.Emote);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((UserId.GetHashCode() * 397) ^ MessageId.GetHashCode()) * 397) ^ Emote.GetHashCode();
	}
}

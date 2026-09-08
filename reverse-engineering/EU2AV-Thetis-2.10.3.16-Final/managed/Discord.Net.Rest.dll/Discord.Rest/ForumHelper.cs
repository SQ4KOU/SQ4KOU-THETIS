using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class ForumHelper
{
	public static Task<Channel> ModifyAsync(IForumChannel channel, BaseDiscordClient client, Action<ForumChannelProperties> func, RequestOptions options)
	{
		ForumChannelProperties forumChannelProperties = new ForumChannelProperties();
		func(forumChannelProperties);
		Preconditions.AtMost(forumChannelProperties.Tags.IsSpecified ? forumChannelProperties.Tags.Value.Count() : 0, 20, "Tags", "Forum channel can have max 20 tags.");
		ModifyForumChannelParams args = new ModifyForumChannelParams
		{
			Name = forumChannelProperties.Name,
			Position = forumChannelProperties.Position,
			CategoryId = forumChannelProperties.CategoryId,
			Overwrites = (forumChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)forumChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			DefaultSlowModeInterval = forumChannelProperties.DefaultSlowModeInterval,
			ThreadCreationInterval = forumChannelProperties.ThreadCreationInterval,
			Tags = (forumChannelProperties.Tags.IsSpecified ? ((Optional<ModifyForumTagParams[]>)forumChannelProperties.Tags.Value.Select((IForumTag tag) => new ModifyForumTagParams
			{
				Id = (((Optional<ulong>?)tag.Id) ?? Optional<ulong>.Unspecified),
				Name = tag.Name,
				EmojiId = ((tag.Emoji is Emote emote2) ? ((Optional<ulong?>)emote2.Id) : Optional<ulong?>.Unspecified),
				EmojiName = ((tag.Emoji is Emoji emoji2) ? ((Optional<string>)emoji2.Name) : Optional<string>.Unspecified)
			}).ToArray()) : Optional.Create<ModifyForumTagParams[]>()),
			Flags = forumChannelProperties.Flags.GetValueOrDefault(),
			Topic = forumChannelProperties.Topic,
			DefaultReactionEmoji = (forumChannelProperties.DefaultReactionEmoji.IsSpecified ? ((Optional<ModifyForumReactionEmojiParams>)new ModifyForumReactionEmojiParams
			{
				EmojiId = ((forumChannelProperties.DefaultReactionEmoji.Value is Emote emote) ? ((Optional<ulong?>)emote.Id) : Optional<ulong?>.Unspecified),
				EmojiName = ((forumChannelProperties.DefaultReactionEmoji.Value is Emoji emoji) ? ((Optional<string>)emoji.Name) : Optional<string>.Unspecified)
			}) : Optional<ModifyForumReactionEmojiParams>.Unspecified),
			DefaultSortOrder = forumChannelProperties.DefaultSortOrder
		};
		return client.ApiClient.ModifyGuildChannelAsync(channel.Id, args, options);
	}
}

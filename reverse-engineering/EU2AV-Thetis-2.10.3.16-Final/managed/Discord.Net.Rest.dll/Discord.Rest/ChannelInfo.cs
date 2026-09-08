using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public struct ChannelInfo
{
	public string Name { get; }

	public string Topic { get; }

	public int? SlowModeInterval { get; }

	public bool? IsNsfw { get; }

	public int? Bitrate { get; }

	public ChannelType? ChannelType { get; }

	public ThreadArchiveDuration? DefaultArchiveDuration { get; }

	public IReadOnlyCollection<ForumTag> ForumTags { get; }

	public IEmote DefaultReactionEmoji { get; }

	public int? UserLimit { get; }

	public VideoQualityMode? VideoQualityMode { get; }

	public string RtcRegion { get; }

	public ChannelFlags? Flags { get; }

	public ThreadArchiveDuration? AutoArchiveDuration { get; }

	public int? DefaultSlowModeInterval { get; }

	internal ChannelInfo(ChannelInfoAuditLogModel model)
	{
		Name = model.Name;
		Topic = model.Topic;
		IsNsfw = model.IsNsfw;
		Bitrate = model.Bitrate;
		DefaultArchiveDuration = model.DefaultArchiveDuration;
		ChannelType = model.Type;
		SlowModeInterval = model.RateLimitPerUser;
		ForumTags = model.AvailableTags?.Select((Discord.API.ForumTag x) => new ForumTag(x.Id, x.Name, x.EmojiId.GetValueOrDefault(null), x.EmojiName.GetValueOrDefault(null), x.Moderated)).ToImmutableArray();
		if (model.DefaultEmoji != null)
		{
			if (model.DefaultEmoji.EmojiId.HasValue && model.DefaultEmoji.EmojiId.Value != 0L)
			{
				DefaultReactionEmoji = new Emote(model.DefaultEmoji.EmojiId.GetValueOrDefault(), null, false);
			}
			else if (model.DefaultEmoji.EmojiName.IsSpecified)
			{
				DefaultReactionEmoji = new Emoji(model.DefaultEmoji.EmojiName.Value);
			}
			else
			{
				DefaultReactionEmoji = null;
			}
		}
		else
		{
			DefaultReactionEmoji = null;
		}
		AutoArchiveDuration = model.AutoArchiveDuration;
		DefaultSlowModeInterval = model.DefaultThreadRateLimitPerUser;
		VideoQualityMode = model.VideoQualityMode;
		RtcRegion = model.Region;
		Flags = model.Flags;
		UserLimit = model.UserLimit;
	}
}

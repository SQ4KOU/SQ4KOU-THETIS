using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ChannelCreateAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public string ChannelName { get; }

	public ChannelType ChannelType { get; }

	public int? SlowModeInterval { get; }

	public bool? IsNsfw { get; }

	public int? Bitrate { get; }

	public IReadOnlyCollection<Overwrite> Overwrites { get; }

	public ThreadArchiveDuration? AutoArchiveDuration { get; }

	public ThreadArchiveDuration? DefaultAutoArchiveDuration { get; }

	public int? DefaultSlowModeInterval { get; }

	public string Topic { get; }

	public IReadOnlyCollection<ForumTag> AvailableTags { get; }

	public IEmote DefaultReactionEmoji { get; }

	public int? UserLimit { get; }

	public VideoQualityMode? VideoQualityMode { get; }

	public string RtcRegion { get; }

	public ChannelFlags? Flags { get; }

	private ChannelCreateAuditLogData(ChannelInfoAuditLogModel model, AuditLogEntry entry)
	{
		ChannelId = entry.TargetId.Value;
		ChannelName = model.Name;
		ChannelType = model.Type.Value;
		SlowModeInterval = model.RateLimitPerUser;
		IsNsfw = model.IsNsfw;
		Bitrate = model.Bitrate;
		Topic = model.Topic;
		AutoArchiveDuration = model.AutoArchiveDuration;
		DefaultSlowModeInterval = model.DefaultThreadRateLimitPerUser;
		DefaultAutoArchiveDuration = model.DefaultArchiveDuration;
		AvailableTags = model.AvailableTags?.Select((Discord.API.ForumTag x) => new ForumTag(x.Id, x.Name, x.EmojiId.GetValueOrDefault(null), x.EmojiName.GetValueOrDefault(null), x.Moderated)).ToImmutableArray();
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
		VideoQualityMode = model.VideoQualityMode;
		RtcRegion = model.Region;
		Flags = model.Flags;
		UserLimit = model.UserLimit;
		Overwrites = model.Overwrites?.Select((Discord.API.Overwrite x) => new Overwrite(x.TargetId, x.TargetType, new OverwritePermissions(x.Allow, x.Deny))).ToImmutableArray();
	}

	internal static ChannelCreateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		return new ChannelCreateAuditLogData(AuditLogHelper.CreateAuditLogEntityInfo<ChannelInfoAuditLogModel>(entry.Changes, discord).Item2, entry);
	}
}

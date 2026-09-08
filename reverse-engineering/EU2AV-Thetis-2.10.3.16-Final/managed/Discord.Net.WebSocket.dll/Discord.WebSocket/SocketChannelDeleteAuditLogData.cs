using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketChannelDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; }

	public string ChannelName { get; }

	public ChannelType? ChannelType { get; }

	public int? SlowModeInterval { get; }

	public bool? IsNsfw { get; }

	public int? Bitrate { get; }

	public IReadOnlyCollection<Overwrite> Overwrites { get; }

	public int? UserLimit { get; }

	public VideoQualityMode? VideoQualityMode { get; }

	public string RtcRegion { get; }

	public ChannelFlags? Flags { get; }

	public ThreadArchiveDuration? AutoArchiveDuration { get; }

	public int? DefaultSlowModeInterval { get; }

	public ThreadArchiveDuration? DefaultArchiveDuration { get; }

	public IReadOnlyCollection<ForumTag> ForumTags { get; }

	public string Topic { get; }

	public IEmote DefaultReactionEmoji { get; }

	private SocketChannelDeleteAuditLogData(ChannelInfoAuditLogModel model, AuditLogEntry entry)
	{
		ChannelId = entry.TargetId.Value;
		ChannelType = model.Type;
		ChannelName = model.Name;
		Topic = model.Topic;
		IsNsfw = model.IsNsfw;
		Bitrate = model.Bitrate;
		DefaultArchiveDuration = model.DefaultArchiveDuration;
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
		Overwrites = model.Overwrites?.Select((Discord.API.Overwrite x) => new Overwrite(x.TargetId, x.TargetType, new OverwritePermissions(x.Allow, x.Deny))).ToImmutableArray();
	}

	internal static SocketChannelDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketChannelDeleteAuditLogData(AuditLogHelper.CreateAuditLogEntityInfo<ChannelInfoAuditLogModel>(entry.Changes, discord).Item1, entry);
	}
}

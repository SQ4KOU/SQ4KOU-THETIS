using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketStageInstanceUpdatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong StageChannelId { get; }

	public SocketStageInfo Before { get; }

	public SocketStageInfo After { get; }

	internal SocketStageInstanceUpdatedAuditLogData(ulong channelId, SocketStageInfo before, SocketStageInfo after)
	{
		StageChannelId = channelId;
		Before = before;
		After = after;
	}

	internal static SocketStageInstanceUpdatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		ulong value = entry.Options.ChannelId.Value;
		AuditLogChange auditLogChange = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "topic");
		AuditLogChange auditLogChange2 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "privacy");
		string topic = auditLogChange?.OldValue.ToObject<string>();
		string topic2 = auditLogChange?.NewValue.ToObject<string>();
		StagePrivacyLevel? level = auditLogChange2?.OldValue.ToObject<StagePrivacyLevel>();
		return new SocketStageInstanceUpdatedAuditLogData(after: new SocketStageInfo(auditLogChange2?.NewValue.ToObject<StagePrivacyLevel>(), topic2), channelId: value, before: new SocketStageInfo(level, topic));
	}
}

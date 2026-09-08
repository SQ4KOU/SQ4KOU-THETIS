using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketStageInstanceDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public string Topic { get; }

	public StagePrivacyLevel PrivacyLevel { get; }

	public ulong StageChannelId { get; }

	internal SocketStageInstanceDeleteAuditLogData(string topic, StagePrivacyLevel privacyLevel, ulong channelId)
	{
		Topic = topic;
		PrivacyLevel = privacyLevel;
		StageChannelId = channelId;
	}

	internal static SocketStageInstanceDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		string? topic = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "topic").OldValue.ToObject<string>(discord.ApiClient.Serializer);
		StagePrivacyLevel privacyLevel = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "privacy_level").OldValue.ToObject<StagePrivacyLevel>(discord.ApiClient.Serializer);
		return new SocketStageInstanceDeleteAuditLogData(topic, privacyLevel, entry.Options.ChannelId.GetValueOrDefault());
	}
}

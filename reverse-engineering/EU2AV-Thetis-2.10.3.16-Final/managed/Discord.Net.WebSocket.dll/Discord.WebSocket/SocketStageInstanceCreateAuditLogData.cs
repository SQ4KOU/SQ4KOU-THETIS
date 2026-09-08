using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketStageInstanceCreateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public string Topic { get; }

	public StagePrivacyLevel PrivacyLevel { get; }

	public ulong StageChannelId { get; }

	internal SocketStageInstanceCreateAuditLogData(string topic, StagePrivacyLevel privacyLevel, ulong channelId)
	{
		Topic = topic;
		PrivacyLevel = privacyLevel;
		StageChannelId = channelId;
	}

	internal static SocketStageInstanceCreateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		string? topic = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "topic").NewValue.ToObject<string>(discord.ApiClient.Serializer);
		StagePrivacyLevel privacyLevel = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "privacy_level").NewValue.ToObject<StagePrivacyLevel>(discord.ApiClient.Serializer);
		return new SocketStageInstanceCreateAuditLogData(topic, privacyLevel, entry.Options.ChannelId.GetValueOrDefault());
	}
}

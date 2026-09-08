using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketEmoteDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong EmoteId { get; }

	public string Name { get; }

	private SocketEmoteDeleteAuditLogData(ulong id, string name)
	{
		EmoteId = id;
		Name = name;
	}

	internal static SocketEmoteDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		string name = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name").OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new SocketEmoteDeleteAuditLogData(entry.TargetId.Value, name);
	}
}

using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketEmoteCreateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong EmoteId { get; }

	public string Name { get; }

	private SocketEmoteCreateAuditLogData(ulong id, string name)
	{
		EmoteId = id;
		Name = name;
	}

	internal static SocketEmoteCreateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		string name = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name").NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new SocketEmoteCreateAuditLogData(entry.TargetId.Value, name);
	}
}

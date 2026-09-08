using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketEmoteUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong EmoteId { get; }

	public string NewName { get; }

	public string OldName { get; }

	private SocketEmoteUpdateAuditLogData(ulong id, string oldName, string newName)
	{
		EmoteId = id;
		OldName = oldName;
		NewName = newName;
	}

	internal static SocketEmoteUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		AuditLogChange auditLogChange = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		string newName = auditLogChange.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		string oldName = auditLogChange.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new SocketEmoteUpdateAuditLogData(entry.TargetId.Value, oldName, newName);
	}
}

using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class EmoteUpdateAuditLogData : IAuditLogData
{
	public ulong EmoteId { get; }

	public string NewName { get; }

	public string OldName { get; }

	private EmoteUpdateAuditLogData(ulong id, string oldName, string newName)
	{
		EmoteId = id;
		OldName = oldName;
		NewName = newName;
	}

	internal static EmoteUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		AuditLogChange auditLogChange = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		string newName = auditLogChange.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		string oldName = auditLogChange.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new EmoteUpdateAuditLogData(entry.TargetId.Value, oldName, newName);
	}
}

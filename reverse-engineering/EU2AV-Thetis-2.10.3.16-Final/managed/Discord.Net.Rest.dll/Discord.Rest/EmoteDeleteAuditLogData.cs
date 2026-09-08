using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class EmoteDeleteAuditLogData : IAuditLogData
{
	public ulong EmoteId { get; }

	public string Name { get; }

	private EmoteDeleteAuditLogData(ulong id, string name)
	{
		EmoteId = id;
		Name = name;
	}

	internal static EmoteDeleteAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		string name = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name").OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new EmoteDeleteAuditLogData(entry.TargetId.Value, name);
	}
}

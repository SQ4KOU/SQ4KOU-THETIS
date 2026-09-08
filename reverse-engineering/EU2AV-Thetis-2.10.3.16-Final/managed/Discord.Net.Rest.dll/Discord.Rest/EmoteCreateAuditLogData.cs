using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class EmoteCreateAuditLogData : IAuditLogData
{
	public ulong EmoteId { get; }

	public string Name { get; }

	private EmoteCreateAuditLogData(ulong id, string name)
	{
		EmoteId = id;
		Name = name;
	}

	internal static EmoteCreateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		string name = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name").NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new EmoteCreateAuditLogData(entry.TargetId.Value, name);
	}
}

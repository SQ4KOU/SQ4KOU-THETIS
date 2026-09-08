using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class OverwriteUpdateAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public OverwritePermissions OldPermissions { get; }

	public OverwritePermissions NewPermissions { get; }

	public ulong OverwriteTargetId { get; }

	public PermissionTarget OverwriteType { get; }

	private OverwriteUpdateAuditLogData(ulong channelId, OverwritePermissions before, OverwritePermissions after, ulong targetId, PermissionTarget targetType)
	{
		ChannelId = channelId;
		OldPermissions = before;
		NewPermissions = after;
		OverwriteTargetId = targetId;
		OverwriteType = targetType;
	}

	internal static OverwriteUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "deny");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "allow");
		ulong? num = auditLogChange2?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? num2 = auditLogChange2?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? num3 = auditLogChange?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? num4 = auditLogChange?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		OverwritePermissions before = new OverwritePermissions(num.GetValueOrDefault(), num3.GetValueOrDefault());
		OverwritePermissions after = new OverwritePermissions(num2.GetValueOrDefault(), num4.GetValueOrDefault());
		PermissionTarget overwriteType = entry.Options.OverwriteType;
		return new OverwriteUpdateAuditLogData(entry.TargetId.Value, before, after, entry.Options.OverwriteTargetId.Value, overwriteType);
	}
}

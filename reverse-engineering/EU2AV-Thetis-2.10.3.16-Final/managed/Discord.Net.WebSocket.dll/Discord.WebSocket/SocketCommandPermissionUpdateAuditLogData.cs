using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.API;

namespace Discord.WebSocket;

public class SocketCommandPermissionUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ApplicationId { get; set; }

	public ulong CommandId { get; }

	public IReadOnlyCollection<ApplicationCommandPermission> Before { get; }

	public IReadOnlyCollection<ApplicationCommandPermission> After { get; }

	internal SocketCommandPermissionUpdateAuditLogData(IReadOnlyCollection<ApplicationCommandPermission> before, IReadOnlyCollection<ApplicationCommandPermission> after, ulong commandId, ulong appId)
	{
		Before = before;
		After = after;
		ApplicationId = appId;
		CommandId = commandId;
	}

	internal static SocketCommandPermissionUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		List<ApplicationCommandPermission> list = new List<ApplicationCommandPermission>();
		List<ApplicationCommandPermission> list2 = new List<ApplicationCommandPermission>();
		AuditLogChange[] array = changes;
		foreach (AuditLogChange obj in array)
		{
			ApplicationCommandPermissions applicationCommandPermissions = obj.OldValue?.ToObject<ApplicationCommandPermissions>();
			ApplicationCommandPermissions applicationCommandPermissions2 = obj.NewValue?.ToObject<ApplicationCommandPermissions>();
			if (applicationCommandPermissions != null)
			{
				list.Add(new ApplicationCommandPermission(applicationCommandPermissions.Id, applicationCommandPermissions.Type, applicationCommandPermissions.Permission));
			}
			if (applicationCommandPermissions2 != null)
			{
				list2.Add(new ApplicationCommandPermission(applicationCommandPermissions2.Id, applicationCommandPermissions2.Type, applicationCommandPermissions2.Permission));
			}
		}
		return new SocketCommandPermissionUpdateAuditLogData(list.ToImmutableArray(), list2.ToImmutableArray(), entry.TargetId.Value, entry.Options.ApplicationId.Value);
	}
}

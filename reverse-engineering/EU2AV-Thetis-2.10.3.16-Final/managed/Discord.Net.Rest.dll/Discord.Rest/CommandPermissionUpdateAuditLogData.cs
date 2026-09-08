using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class CommandPermissionUpdateAuditLogData : IAuditLogData
{
	public ulong ApplicationId { get; set; }

	public IApplicationCommand ApplicationCommand { get; }

	public IReadOnlyCollection<ApplicationCommandPermission> Before { get; }

	public IReadOnlyCollection<ApplicationCommandPermission> After { get; }

	internal CommandPermissionUpdateAuditLogData(IReadOnlyCollection<ApplicationCommandPermission> before, IReadOnlyCollection<ApplicationCommandPermission> after, IApplicationCommand command, ulong appId)
	{
		Before = before;
		After = after;
		ApplicationCommand = command;
		ApplicationId = appId;
	}

	internal static CommandPermissionUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
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
		ApplicationCommand applicationCommand = log.Commands.FirstOrDefault((ApplicationCommand x) => x.Id == entry.TargetId);
		RestApplicationCommand command = RestApplicationCommand.Create(discord, applicationCommand, (applicationCommand != null && applicationCommand.GuildId.IsSpecified) ? new ulong?(applicationCommand.GuildId.Value) : ((ulong?)null));
		return new CommandPermissionUpdateAuditLogData(list.ToImmutableArray(), list2.ToImmutableArray(), command, entry.Options.ApplicationId.Value);
	}
}

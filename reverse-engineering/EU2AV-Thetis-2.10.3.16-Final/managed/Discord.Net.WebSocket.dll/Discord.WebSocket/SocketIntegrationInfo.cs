using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.API.AuditLogs;

namespace Discord.WebSocket;

public class SocketIntegrationInfo
{
	public string Name { get; set; }

	public string Type { get; set; }

	public bool? Enabled { get; set; }

	public bool? Syncing { get; set; }

	public ulong? RoleId { get; set; }

	public bool? EnableEmojis { get; set; }

	public IntegrationExpireBehavior? ExpireBehavior { get; set; }

	public int? ExpireGracePeriod { get; set; }

	public IReadOnlyCollection<string> Scopes { get; set; }

	internal SocketIntegrationInfo(IntegrationInfoAuditLogModel model)
	{
		Name = model.Name;
		Type = model.Type;
		EnableEmojis = model.EnableEmojis;
		Enabled = model.Enabled;
		Scopes = model.Scopes?.ToImmutableArray();
		ExpireBehavior = model.ExpireBehavior;
		ExpireGracePeriod = model.ExpireGracePeriod;
		Syncing = model.Syncing;
		RoleId = model.RoleId;
	}
}

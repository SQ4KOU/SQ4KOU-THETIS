using Discord.API.AuditLogs;

namespace Discord.Rest;

public struct RoleEditInfo
{
	public Color? Color { get; }

	public bool? Mentionable { get; }

	public bool? Hoist { get; }

	public string Name { get; }

	public GuildPermissions? Permissions { get; }

	public string IconId { get; }

	internal RoleEditInfo(RoleInfoAuditLogModel model)
	{
		if (model.Color.HasValue)
		{
			Color = new Color(model.Color.Value);
		}
		else
		{
			Color = null;
		}
		Mentionable = model.IsMentionable;
		Hoist = model.Hoist;
		Name = model.Name;
		if (model.Permissions.HasValue)
		{
			Permissions = new GuildPermissions(model.Permissions.Value);
		}
		else
		{
			Permissions = null;
		}
		IconId = model.IconHash;
	}
}

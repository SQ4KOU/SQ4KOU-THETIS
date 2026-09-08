using System.Collections.Generic;
using System.Linq;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketMemberRoleAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public IReadOnlyCollection<SocketMemberRoleEditInfo> Roles { get; }

	public Cacheable<SocketUser, RestUser, IUser, ulong> Target { get; }

	public string IntegrationType { get; }

	private SocketMemberRoleAuditLogData(IReadOnlyCollection<SocketMemberRoleEditInfo> roles, Cacheable<SocketUser, RestUser, IUser, ulong> target, string integrationType)
	{
		Roles = roles;
		Target = target;
		IntegrationType = integrationType;
	}

	internal static SocketMemberRoleAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		List<SocketMemberRoleEditInfo> source = (from x in entry.Changes.SelectMany((AuditLogChange x) => x.NewValue.ToObject<Role[]>(discord.ApiClient.Serializer), (AuditLogChange model, Role role) => new
			{
				ChangedProperty = model.ChangedProperty,
				Role = role
			})
			select new SocketMemberRoleEditInfo(x.Role.Name, x.Role.Id, x.ChangedProperty == "$add", x.ChangedProperty == "$remove")).ToList();
		SocketUser user = discord.GetUser(entry.TargetId.Value);
		Cacheable<SocketUser, RestUser, IUser, ulong> target = new Cacheable<SocketUser, RestUser, IUser, ulong>(user, entry.TargetId.Value, user != null, async delegate
		{
			User user2 = await discord.ApiClient.GetUserAsync(entry.TargetId.Value);
			return (user2 != null) ? RestUser.Create(discord, user2) : null;
		});
		return new SocketMemberRoleAuditLogData(source.ToReadOnlyCollection(), target, entry.Options?.IntegrationType);
	}
}

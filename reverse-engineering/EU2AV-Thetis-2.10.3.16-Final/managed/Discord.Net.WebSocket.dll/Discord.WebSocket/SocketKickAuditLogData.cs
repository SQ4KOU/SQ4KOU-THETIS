using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketKickAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public Cacheable<SocketUser, RestUser, IUser, ulong> Target { get; }

	public string IntegrationType { get; }

	private SocketKickAuditLogData(Cacheable<SocketUser, RestUser, IUser, ulong> user, string integrationType)
	{
		Target = user;
		IntegrationType = integrationType;
	}

	internal static SocketKickAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		SocketUser user = discord.GetUser(entry.TargetId.Value);
		return new SocketKickAuditLogData(new Cacheable<SocketUser, RestUser, IUser, ulong>(user, entry.TargetId.Value, user != null, async delegate
		{
			User user2 = await discord.ApiClient.GetUserAsync(entry.TargetId.Value);
			return (user2 != null) ? RestUser.Create(discord, user2) : null;
		}), entry.Options?.IntegrationType);
	}
}

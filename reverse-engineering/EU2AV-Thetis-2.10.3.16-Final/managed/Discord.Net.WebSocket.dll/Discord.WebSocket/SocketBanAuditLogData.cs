using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketBanAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public Cacheable<SocketUser, RestUser, IUser, ulong> Target { get; }

	private SocketBanAuditLogData(Cacheable<SocketUser, RestUser, IUser, ulong> user)
	{
		Target = user;
	}

	internal static SocketBanAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		SocketUser user = discord.GetUser(entry.TargetId.Value);
		return new SocketBanAuditLogData(new Cacheable<SocketUser, RestUser, IUser, ulong>(user, entry.TargetId.Value, user != null, async delegate
		{
			User user2 = await discord.ApiClient.GetUserAsync(entry.TargetId.Value);
			return (user2 != null) ? RestUser.Create(discord, user2) : null;
		}));
	}
}

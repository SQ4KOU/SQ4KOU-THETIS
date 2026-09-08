using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketBotAddAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public Cacheable<SocketUser, RestUser, IUser, ulong> Target { get; }

	private SocketBotAddAuditLogData(Cacheable<SocketUser, RestUser, IUser, ulong> bot)
	{
		Target = bot;
	}

	internal static SocketBotAddAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		SocketUser user = discord.GetUser(entry.TargetId.Value);
		return new SocketBotAddAuditLogData(new Cacheable<SocketUser, RestUser, IUser, ulong>(user, entry.TargetId.Value, user != null, async delegate
		{
			User user2 = await discord.ApiClient.GetUserAsync(entry.TargetId.Value);
			return (user2 != null) ? RestUser.Create(discord, user2) : null;
		}));
	}
}

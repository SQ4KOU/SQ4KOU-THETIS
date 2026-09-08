using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketMessageDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public int MessageCount { get; }

	public ulong ChannelId { get; }

	public Cacheable<SocketUser, RestUser, IUser, ulong> Target { get; }

	private SocketMessageDeleteAuditLogData(ulong channelId, int count, Cacheable<SocketUser, RestUser, IUser, ulong> user)
	{
		ChannelId = channelId;
		MessageCount = count;
		Target = user;
	}

	internal static SocketMessageDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		SocketUser user = discord.GetUser(entry.TargetId.Value);
		Cacheable<SocketUser, RestUser, IUser, ulong> user2 = new Cacheable<SocketUser, RestUser, IUser, ulong>(user, entry.TargetId.Value, user != null, async delegate
		{
			User user3 = await discord.ApiClient.GetUserAsync(entry.TargetId.Value);
			return (user3 != null) ? RestUser.Create(discord, user3) : null;
		});
		return new SocketMessageDeleteAuditLogData(entry.Options.ChannelId.Value, entry.Options.Count.Value, user2);
	}
}

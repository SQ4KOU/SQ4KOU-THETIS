using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketMessageUnpinAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong MessageId { get; }

	public ulong ChannelId { get; }

	public Cacheable<SocketUser, RestUser, IUser, ulong>? Target { get; }

	private SocketMessageUnpinAuditLogData(ulong messageId, ulong channelId, Cacheable<SocketUser, RestUser, IUser, ulong>? user)
	{
		MessageId = messageId;
		ChannelId = channelId;
		Target = user;
	}

	internal static SocketMessageUnpinAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		Cacheable<SocketUser, RestUser, IUser, ulong>? user = null;
		if (entry.TargetId.HasValue)
		{
			SocketUser user2 = discord.GetUser(entry.TargetId.Value);
			user = new Cacheable<SocketUser, RestUser, IUser, ulong>(user2, entry.TargetId.Value, user2 != null, async delegate
			{
				User user3 = await discord.ApiClient.GetUserAsync(entry.TargetId.Value);
				return (user3 != null) ? RestUser.Create(discord, user3) : null;
			});
		}
		return new SocketMessageUnpinAuditLogData(entry.Options.MessageId.Value, entry.Options.ChannelId.Value, user);
	}
}

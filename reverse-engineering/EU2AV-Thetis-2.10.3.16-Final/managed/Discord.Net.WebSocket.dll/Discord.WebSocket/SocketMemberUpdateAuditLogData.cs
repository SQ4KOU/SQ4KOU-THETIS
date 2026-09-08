using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketMemberUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public Cacheable<SocketUser, RestUser, IUser, ulong> Target { get; }

	public MemberInfo Before { get; }

	public MemberInfo After { get; }

	private SocketMemberUpdateAuditLogData(Cacheable<SocketUser, RestUser, IUser, ulong> target, MemberInfo before, MemberInfo after)
	{
		Target = target;
		Before = before;
		After = after;
	}

	internal static SocketMemberUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		(MemberInfoAuditLogModel, MemberInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<MemberInfoAuditLogModel>(entry.Changes, discord);
		MemberInfoAuditLogModel item = tuple.Item1;
		MemberInfoAuditLogModel item2 = tuple.Item2;
		SocketUser user = discord.GetUser(entry.TargetId.Value);
		return new SocketMemberUpdateAuditLogData(new Cacheable<SocketUser, RestUser, IUser, ulong>(user, entry.TargetId.Value, user != null, async delegate
		{
			User user2 = await discord.ApiClient.GetUserAsync(entry.TargetId.Value);
			return (user2 != null) ? RestUser.Create(discord, user2) : null;
		}), new MemberInfo(item), new MemberInfo(item2));
	}
}

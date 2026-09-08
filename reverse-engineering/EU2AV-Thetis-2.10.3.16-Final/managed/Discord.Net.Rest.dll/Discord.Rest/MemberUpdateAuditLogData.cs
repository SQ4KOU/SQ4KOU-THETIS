using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class MemberUpdateAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	public MemberInfo Before { get; }

	public MemberInfo After { get; }

	private MemberUpdateAuditLogData(IUser target, MemberInfo before, MemberInfo after)
	{
		Target = target;
		Before = before;
		After = after;
	}

	internal static MemberUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		(MemberInfoAuditLogModel, MemberInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<MemberInfoAuditLogModel>(entry.Changes, discord);
		MemberInfoAuditLogModel item = tuple.Item1;
		MemberInfoAuditLogModel item2 = tuple.Item2;
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new MemberUpdateAuditLogData((user != null) ? RestUser.Create(discord, user) : null, new MemberInfo(item), new MemberInfo(item2));
	}
}

using System;
using Discord.API.Gateway;

namespace Discord.WebSocket;

public class SocketAuditLogEntry : SocketEntity<ulong>, IAuditLogEntry, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public ActionType Action { get; }

	public ISocketAuditLogData Data { get; }

	public SocketUser User { get; private set; }

	public string Reason { get; }

	IUser IAuditLogEntry.User => User;

	IAuditLogData IAuditLogEntry.Data => Data;

	private SocketAuditLogEntry(DiscordSocketClient discord, AuditLogCreatedEvent model)
		: base(discord, model.Id)
	{
		Action = model.Action;
		Data = SocketAuditLogHelper.CreateData(discord, model);
		Reason = model.Reason;
		User = discord.State.GetGuild(model.GuildId)?.GetUser(model.UserId.GetValueOrDefault());
	}

	internal static SocketAuditLogEntry Create(DiscordSocketClient discord, AuditLogCreatedEvent model)
	{
		return new SocketAuditLogEntry(discord, model);
	}
}

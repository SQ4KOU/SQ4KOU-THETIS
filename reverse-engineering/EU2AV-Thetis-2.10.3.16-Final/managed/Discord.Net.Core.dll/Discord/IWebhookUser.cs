namespace Discord;

public interface IWebhookUser : IGuildUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	ulong WebhookId { get; }
}

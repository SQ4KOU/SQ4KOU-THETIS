namespace Discord;

public interface IGroupUser : IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
}

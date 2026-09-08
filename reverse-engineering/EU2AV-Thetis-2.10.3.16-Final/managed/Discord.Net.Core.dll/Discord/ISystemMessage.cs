namespace Discord;

public interface ISystemMessage : IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
}

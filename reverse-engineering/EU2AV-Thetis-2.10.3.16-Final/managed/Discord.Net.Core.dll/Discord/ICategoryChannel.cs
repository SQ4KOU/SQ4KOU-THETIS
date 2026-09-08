namespace Discord;

public interface ICategoryChannel : IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
}

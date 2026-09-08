namespace Discord;

public interface IMediaChannel : IForumChannel, IMentionable, INestedChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable, IIntegrationChannel
{
}

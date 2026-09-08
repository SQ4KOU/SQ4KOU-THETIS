using System.Threading.Tasks;

namespace Discord;

public interface INewsChannel : ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel
{
	Task<ulong> FollowAnnouncementChannelAsync(ulong channelId, RequestOptions options = null);
}

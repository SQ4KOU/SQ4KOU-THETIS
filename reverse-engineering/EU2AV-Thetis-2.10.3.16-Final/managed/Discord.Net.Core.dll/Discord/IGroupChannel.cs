using System.Threading.Tasks;

namespace Discord;

public interface IGroupChannel : IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IPrivateChannel, IAudioChannel
{
	Task LeaveAsync(RequestOptions options = null);
}

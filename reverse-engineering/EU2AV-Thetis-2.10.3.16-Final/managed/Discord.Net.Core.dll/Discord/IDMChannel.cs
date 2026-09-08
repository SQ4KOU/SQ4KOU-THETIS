using System.Threading.Tasks;

namespace Discord;

public interface IDMChannel : IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IPrivateChannel
{
	IUser Recipient { get; }

	Task CloseAsync(RequestOptions options = null);
}

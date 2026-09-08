using System.Collections.Generic;

namespace Discord.WebSocket;

public interface ISocketPrivateChannel : IPrivateChannel, IChannel, ISnowflakeEntity, IEntity<ulong>
{
	new IReadOnlyCollection<SocketUser> Recipients { get; }
}

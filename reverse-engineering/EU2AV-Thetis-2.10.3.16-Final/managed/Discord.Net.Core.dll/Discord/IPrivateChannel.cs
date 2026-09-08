using System.Collections.Generic;

namespace Discord;

public interface IPrivateChannel : IChannel, ISnowflakeEntity, IEntity<ulong>
{
	IReadOnlyCollection<IUser> Recipients { get; }
}

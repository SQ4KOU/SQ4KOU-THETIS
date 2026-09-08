using System.Collections.Generic;

namespace Discord.Rest;

public interface IRestPrivateChannel : IPrivateChannel, IChannel, ISnowflakeEntity, IEntity<ulong>
{
	new IReadOnlyCollection<RestUser> Recipients { get; }
}

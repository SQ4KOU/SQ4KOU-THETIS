using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface IChannel : ISnowflakeEntity, IEntity<ulong>
{
	ChannelType ChannelType { get; }

	string Name { get; }

	IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord;

public interface IIntegrationChannel : IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	Task<IWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null);

	Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);

	Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null);
}

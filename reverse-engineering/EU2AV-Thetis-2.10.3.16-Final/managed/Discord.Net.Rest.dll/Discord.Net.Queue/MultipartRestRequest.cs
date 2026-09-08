using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Net.Rest;

namespace Discord.Net.Queue;

public class MultipartRestRequest : RestRequest
{
	public IReadOnlyDictionary<string, object> MultipartParams { get; }

	public MultipartRestRequest(IRestClient client, string method, string endpoint, IReadOnlyDictionary<string, object> multipartParams, RequestOptions options)
		: base(client, method, endpoint, options)
	{
		MultipartParams = multipartParams;
	}

	public override Task<RestResponse> SendAsync()
	{
		return base.Client.SendAsync(base.Method, base.Endpoint, MultipartParams, base.Options.CancelToken, base.Options.HeaderOnly, base.Options.AuditLogReason);
	}
}

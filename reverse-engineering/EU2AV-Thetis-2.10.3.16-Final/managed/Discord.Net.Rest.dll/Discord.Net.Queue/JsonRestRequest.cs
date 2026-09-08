using System.Threading.Tasks;
using Discord.Net.Rest;

namespace Discord.Net.Queue;

public class JsonRestRequest : RestRequest
{
	public string Json { get; }

	public JsonRestRequest(IRestClient client, string method, string endpoint, string json, RequestOptions options)
		: base(client, method, endpoint, options)
	{
		Json = json;
	}

	public override Task<RestResponse> SendAsync()
	{
		return base.Client.SendAsync(base.Method, base.Endpoint, Json, base.Options.CancelToken, base.Options.HeaderOnly, base.Options.AuditLogReason);
	}
}

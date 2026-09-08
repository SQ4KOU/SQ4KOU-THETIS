using System.Threading.Tasks;

namespace Discord.Rest;

internal static class InviteHelper
{
	public static Task DeleteAsync(IInvite invite, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.DeleteInviteAsync(invite.Code, options);
	}
}

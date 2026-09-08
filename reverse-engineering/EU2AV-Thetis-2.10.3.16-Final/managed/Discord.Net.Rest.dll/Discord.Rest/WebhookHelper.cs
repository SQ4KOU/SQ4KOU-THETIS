using System;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class WebhookHelper
{
	public static Task<Webhook> ModifyAsync(IWebhook webhook, BaseDiscordClient client, Action<WebhookProperties> func, RequestOptions options)
	{
		WebhookProperties webhookProperties = new WebhookProperties();
		func(webhookProperties);
		ModifyWebhookParams modifyWebhookParams = new ModifyWebhookParams
		{
			Avatar = (webhookProperties.Image.IsSpecified ? ((Optional<Discord.API.Image?>)(webhookProperties.Image.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Name = webhookProperties.Name
		};
		if (!modifyWebhookParams.Avatar.IsSpecified && webhook.AvatarId != null)
		{
			modifyWebhookParams.Avatar = new Discord.API.Image(webhook.AvatarId);
		}
		if (webhookProperties.Channel.IsSpecified)
		{
			modifyWebhookParams.ChannelId = webhookProperties.Channel.Value.Id;
		}
		else if (webhookProperties.ChannelId.IsSpecified)
		{
			modifyWebhookParams.ChannelId = webhookProperties.ChannelId.Value;
		}
		return client.ApiClient.ModifyWebhookAsync(webhook.Id, modifyWebhookParams, options);
	}

	public static Task DeleteAsync(IWebhook webhook, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.DeleteWebhookAsync(webhook.Id, options);
	}
}

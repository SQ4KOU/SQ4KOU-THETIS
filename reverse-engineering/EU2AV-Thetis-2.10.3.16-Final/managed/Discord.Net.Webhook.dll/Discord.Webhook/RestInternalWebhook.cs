using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Webhook;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestInternalWebhook : IWebhook, IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	private DiscordWebhookClient _client;

	public ulong Id { get; }

	public string Token { get; }

	public ulong? ChannelId { get; private set; }

	public string Name { get; private set; }

	public string AvatarId { get; private set; }

	public ulong? GuildId { get; private set; }

	public ulong? ApplicationId { get; private set; }

	public WebhookType Type { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	private string DebuggerDisplay => $"Webhook: {Name} ({Id})";

	IUser IWebhook.Creator => null;

	IIntegrationChannel IWebhook.Channel => null;

	IGuild IWebhook.Guild => null;

	internal RestInternalWebhook(DiscordWebhookClient apiClient, Discord.API.Webhook model)
	{
		_client = apiClient;
		Id = model.Id;
		ChannelId = model.Id;
		Token = model.Token.GetValueOrDefault(null);
	}

	internal static RestInternalWebhook Create(DiscordWebhookClient client, Discord.API.Webhook model)
	{
		RestInternalWebhook restInternalWebhook = new RestInternalWebhook(client, model);
		restInternalWebhook.Update(model);
		return restInternalWebhook;
	}

	internal void Update(Discord.API.Webhook model)
	{
		if (ChannelId != model.ChannelId)
		{
			ChannelId = model.ChannelId;
		}
		if (model.Avatar.IsSpecified)
		{
			AvatarId = model.Avatar.Value;
		}
		if (model.GuildId.IsSpecified)
		{
			GuildId = model.GuildId.Value;
		}
		if (model.Name.IsSpecified)
		{
			Name = model.Name.Value;
		}
		Type = model.Type;
		ApplicationId = model.ApplicationId;
	}

	public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return CDN.GetUserAvatarUrl(Id, AvatarId, size, format);
	}

	public async Task ModifyAsync(Action<WebhookProperties> func, RequestOptions options = null)
	{
		Update(await WebhookClientHelper.ModifyAsync(_client, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return WebhookClientHelper.DeleteAsync(_client, options);
	}

	public override string ToString()
	{
		return $"Webhook: {Name}:{Id}";
	}
}

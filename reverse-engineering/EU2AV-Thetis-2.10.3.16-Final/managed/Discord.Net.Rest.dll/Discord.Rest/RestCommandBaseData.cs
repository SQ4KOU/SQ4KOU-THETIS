using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

public class RestCommandBaseData<TOption> : RestEntity<ulong>, IApplicationCommandInteractionData, IDiscordInteractionData where TOption : IApplicationCommandInteractionDataOption
{
	internal RestResolvableData<ApplicationCommandInteractionData> ResolvableData;

	public string Name { get; private set; }

	public virtual IReadOnlyCollection<TOption> Options { get; internal set; }

	IReadOnlyCollection<IApplicationCommandInteractionDataOption> IApplicationCommandInteractionData.Options => (IReadOnlyCollection<IApplicationCommandInteractionDataOption>)Options;

	internal RestCommandBaseData(BaseDiscordClient client, ApplicationCommandInteractionData model)
		: base(client, model.Id)
	{
	}

	internal static async Task<RestCommandBaseData> CreateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, ulong? guildId, IRestMessageChannel channel, bool doApiCall)
	{
		RestCommandBaseData entity = new RestCommandBaseData(client, model);
		await entity.UpdateAsync(client, model, guild, guildId, channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}

	internal virtual Task UpdateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, ulong? guildId, IRestMessageChannel channel, bool doApiCall)
	{
		Name = model.Name;
		if (model.Resolved.IsSpecified && ResolvableData == null)
		{
			ResolvableData = new RestResolvableData<ApplicationCommandInteractionData>();
			return ResolvableData.PopulateAsync(client, guild, guildId, channel, model, doApiCall);
		}
		return Task.CompletedTask;
	}
}
public class RestCommandBaseData : RestCommandBaseData<IApplicationCommandInteractionDataOption>
{
	internal RestCommandBaseData(DiscordRestClient client, ApplicationCommandInteractionData model)
		: base((BaseDiscordClient)client, model)
	{
	}
}

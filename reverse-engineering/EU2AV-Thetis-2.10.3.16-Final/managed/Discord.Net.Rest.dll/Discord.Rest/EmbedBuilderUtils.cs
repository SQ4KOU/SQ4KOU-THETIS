using System;
using Discord.API;
using Discord.Net.Converters;
using Newtonsoft.Json;

namespace Discord.Rest;

public static class EmbedBuilderUtils
{
	private static Lazy<JsonSerializerSettings> _settings = new Lazy<JsonSerializerSettings>(() => new JsonSerializerSettings
	{
		ContractResolver = new DiscordContractResolver()
	});

	public static EmbedBuilder Parse(string json)
	{
		try
		{
			Embed embed = JsonConvert.DeserializeObject<Discord.API.Embed>(json, _settings.Value)?.ToEntity();
			if ((object)embed != null)
			{
				return embed.ToEmbedBuilder();
			}
			return new EmbedBuilder();
		}
		catch
		{
			throw;
		}
	}

	public static bool TryParse(string json, out EmbedBuilder builder)
	{
		builder = new EmbedBuilder();
		try
		{
			Embed embed = JsonConvert.DeserializeObject<Discord.API.Embed>(json, _settings.Value)?.ToEntity();
			if ((object)embed != null)
			{
				builder = embed.ToEmbedBuilder();
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}
}

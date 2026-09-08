namespace Discord;

public readonly struct PrimaryGuild
{
	public ulong? GuildId { get; }

	public bool? IdentityEnabled { get; }

	public string Tag { get; }

	public string BadgeHash { get; }

	internal PrimaryGuild(ulong? guildId, bool? identityEnabled, string tag, string badgeHash)
	{
		GuildId = guildId;
		IdentityEnabled = identityEnabled;
		Tag = tag;
		BadgeHash = badgeHash;
	}

	public string GetBadgeUrl()
	{
		if (GuildId.HasValue && BadgeHash != null)
		{
			return CDN.GetGuildTagBadgeUrl(GuildId.Value, BadgeHash);
		}
		return null;
	}
}

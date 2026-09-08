using System;

namespace Discord;

public class PartialGuild : ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	public ulong Id { get; internal set; }

	public string Name { get; internal set; }

	public string Description { get; internal set; }

	public string SplashId { get; internal set; }

	public string SplashUrl => CDN.GetGuildSplashUrl(Id, SplashId, 2048);

	public string BannerId { get; internal set; }

	public string BannerUrl => CDN.GetGuildBannerUrl(Id, BannerId, ImageFormat.Auto);

	public GuildFeatures Features { get; internal set; }

	public string IconId { get; internal set; }

	public string IconUrl => CDN.GetGuildIconUrl(Id, IconId, 2048);

	public VerificationLevel? VerificationLevel { get; internal set; }

	public string VanityURLCode { get; internal set; }

	public int? PremiumSubscriptionCount { get; internal set; }

	public NsfwLevel? NsfwLevel { get; internal set; }

	public WelcomeScreen WelcomeScreen { get; internal set; }

	public int? ApproximateMemberCount { get; internal set; }

	public int? ApproximatePresenceCount { get; internal set; }

	internal PartialGuild()
	{
	}
}

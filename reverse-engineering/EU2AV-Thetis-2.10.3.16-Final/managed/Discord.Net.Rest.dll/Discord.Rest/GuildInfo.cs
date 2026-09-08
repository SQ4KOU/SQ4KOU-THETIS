using Discord.API.AuditLogs;

namespace Discord.Rest;

public struct GuildInfo
{
	public string DiscoverySplashId { get; }

	public string SplashId { get; }

	public ulong? RulesChannelId { get; }

	public ulong? PublicUpdatesChannelId { get; }

	public ulong? OwnerId { get; }

	public ulong? ApplicationId { get; }

	public string BannerId { get; }

	public string VanityURLCode { get; }

	public SystemChannelMessageDeny? SystemChannelFlags { get; }

	public string Description { get; }

	public string PreferredLocale { get; }

	public NsfwLevel? NsfwLevel { get; }

	public bool? IsBoostProgressBarEnabled { get; }

	public int? AfkTimeout { get; }

	public DefaultMessageNotifications? DefaultMessageNotifications { get; }

	public ulong? AfkChannelId { get; }

	public string Name { get; }

	public string RegionId { get; }

	public string IconHash { get; }

	public VerificationLevel? VerificationLevel { get; }

	public IUser Owner { get; }

	public MfaLevel? MfaLevel { get; }

	public ExplicitContentFilterLevel? ExplicitContentFilter { get; }

	public ulong? SystemChannelId { get; }

	public ulong? EmbedChannelId { get; }

	public bool? IsEmbeddable { get; }

	internal GuildInfo(GuildInfoAuditLogModel model, IUser owner)
	{
		Owner = owner;
		Name = model.Name;
		AfkTimeout = model.AfkTimeout.GetValueOrDefault();
		IsEmbeddable = model.IsEmbeddable;
		DefaultMessageNotifications = model.DefaultMessageNotifications;
		MfaLevel = model.MfaLevel;
		Description = model.Description;
		PreferredLocale = model.PreferredLocale;
		IconHash = model.IconHash;
		OwnerId = model.OwnerId;
		AfkChannelId = model.AfkChannelId;
		ApplicationId = model.ApplicationId;
		BannerId = model.Banner;
		DiscoverySplashId = model.DiscoverySplash;
		EmbedChannelId = model.EmbeddedChannelId;
		ExplicitContentFilter = model.ExplicitContentFilterLevel;
		IsBoostProgressBarEnabled = model.ProgressBarEnabled;
		NsfwLevel = model.NsfwLevel;
		PublicUpdatesChannelId = model.PublicUpdatesChannelId;
		RegionId = model.RegionId;
		RulesChannelId = model.RulesChannelId;
		SplashId = model.Splash;
		SystemChannelFlags = model.SystemChannelFlags;
		SystemChannelId = model.SystemChannelId;
		VanityURLCode = model.VanityUrl;
		VerificationLevel = model.VerificationLevel;
	}
}

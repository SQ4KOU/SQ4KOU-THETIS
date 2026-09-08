using Discord.API.AuditLogs;

namespace Discord.WebSocket;

public class SocketGuildInfo
{
	public string Name { get; private set; }

	public int? AfkTimeout { get; private set; }

	public bool? IsEmbeddable { get; private set; }

	public DefaultMessageNotifications? DefaultMessageNotifications { get; private set; }

	public MfaLevel? MfaLevel { get; private set; }

	public VerificationLevel? VerificationLevel { get; private set; }

	public ExplicitContentFilterLevel? ExplicitContentFilter { get; private set; }

	public string IconHash { get; private set; }

	public string SplashId { get; private set; }

	public string DiscoverySplashId { get; private set; }

	public ulong? AfkChannelId { get; private set; }

	public ulong? EmbedChannelId { get; private set; }

	public ulong? SystemChannelId { get; private set; }

	public ulong? RulesChannelId { get; private set; }

	public ulong? PublicUpdatesChannelId { get; private set; }

	public ulong? OwnerId { get; private set; }

	public ulong? ApplicationId { get; private set; }

	public string RegionId { get; private set; }

	public string BannerId { get; private set; }

	public string VanityURLCode { get; private set; }

	public SystemChannelMessageDeny? SystemChannelFlags { get; private set; }

	public string Description { get; private set; }

	public string PreferredLocale { get; private set; }

	public NsfwLevel? NsfwLevel { get; private set; }

	public bool? IsBoostProgressBarEnabled { get; private set; }

	internal static SocketGuildInfo Create(GuildInfoAuditLogModel model)
	{
		return new SocketGuildInfo
		{
			Name = model.Name,
			AfkTimeout = model.AfkTimeout.GetValueOrDefault(),
			IsEmbeddable = model.IsEmbeddable,
			DefaultMessageNotifications = model.DefaultMessageNotifications,
			MfaLevel = model.MfaLevel,
			Description = model.Description,
			PreferredLocale = model.PreferredLocale,
			IconHash = model.IconHash,
			OwnerId = model.OwnerId,
			AfkChannelId = model.AfkChannelId,
			ApplicationId = model.ApplicationId,
			BannerId = model.Banner,
			DiscoverySplashId = model.DiscoverySplash,
			EmbedChannelId = model.EmbeddedChannelId,
			ExplicitContentFilter = model.ExplicitContentFilterLevel,
			IsBoostProgressBarEnabled = model.ProgressBarEnabled,
			NsfwLevel = model.NsfwLevel,
			PublicUpdatesChannelId = model.PublicUpdatesChannelId,
			RegionId = model.RegionId,
			RulesChannelId = model.RulesChannelId,
			SplashId = model.Splash,
			SystemChannelFlags = model.SystemChannelFlags,
			SystemChannelId = model.SystemChannelId,
			VanityURLCode = model.VanityUrl,
			VerificationLevel = model.VerificationLevel
		};
	}
}

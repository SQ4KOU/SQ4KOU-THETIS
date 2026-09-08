using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal static class PartialGuildExtensions
{
	public static PartialGuild Create(Discord.API.PartialGuild model)
	{
		return new PartialGuild
		{
			Id = model.Id,
			Name = model.Name,
			Description = (model.Description.IsSpecified ? model.Description.Value : null),
			SplashId = (model.Splash.IsSpecified ? model.Splash.Value : null),
			BannerId = (model.BannerHash.IsSpecified ? model.BannerHash.Value : null),
			Features = (model.Features.IsSpecified ? model.Features.Value : null),
			IconId = (model.IconHash.IsSpecified ? model.IconHash.Value : null),
			VerificationLevel = (model.VerificationLevel.IsSpecified ? new VerificationLevel?(model.VerificationLevel.Value) : ((VerificationLevel?)null)),
			VanityURLCode = (model.VanityUrlCode.IsSpecified ? model.VanityUrlCode.Value : null),
			PremiumSubscriptionCount = (model.PremiumSubscriptionCount.IsSpecified ? new int?(model.PremiumSubscriptionCount.Value) : ((int?)null)),
			NsfwLevel = (model.NsfwLevel.IsSpecified ? new NsfwLevel?(model.NsfwLevel.Value) : ((NsfwLevel?)null)),
			WelcomeScreen = (model.WelcomeScreen.IsSpecified ? new WelcomeScreen(model.WelcomeScreen.Value.Description.IsSpecified ? model.WelcomeScreen.Value.Description.Value : null, model.WelcomeScreen.Value.WelcomeChannels.Select((Discord.API.WelcomeScreenChannel ch) => new WelcomeScreenChannel(ch.ChannelId, ch.Description, ch.EmojiName.IsSpecified ? ch.EmojiName.Value : null, ch.EmojiId.IsSpecified ? ch.EmojiId.Value : ((ulong?)null))).ToImmutableArray()) : null),
			ApproximateMemberCount = (model.ApproximateMemberCount.IsSpecified ? new int?(model.ApproximateMemberCount.Value) : ((int?)null)),
			ApproximatePresenceCount = (model.ApproximatePresenceCount.IsSpecified ? new int?(model.ApproximatePresenceCount.Value) : ((int?)null))
		};
	}
}

namespace Discord;

public class RoleTags
{
	public ulong? BotId { get; }

	public ulong? IntegrationId { get; }

	public bool IsPremiumSubscriberRole { get; }

	public ulong? SubscriptionListingId { get; }

	public bool IsAvailableForPurchase { get; }

	public bool IsGuildConnection { get; }

	internal RoleTags(ulong? botId, ulong? integrationId, bool isPremiumSubscriber, ulong? subscriptionListingId, bool isAvailableForPurchase, bool isGuildConnection)
	{
		BotId = botId;
		IntegrationId = integrationId;
		IsPremiumSubscriberRole = isPremiumSubscriber;
		SubscriptionListingId = subscriptionListingId;
		IsAvailableForPurchase = isAvailableForPurchase;
		IsGuildConnection = isGuildConnection;
	}
}

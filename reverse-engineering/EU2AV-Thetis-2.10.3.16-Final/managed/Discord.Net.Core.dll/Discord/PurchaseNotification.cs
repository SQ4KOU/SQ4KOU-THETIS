namespace Discord;

public readonly struct PurchaseNotification
{
	public readonly PurchaseType Type;

	public readonly GuildProductPurchase? ProductPurchase;

	internal PurchaseNotification(PurchaseType type, GuildProductPurchase? productPurchase)
	{
		Type = type;
		ProductPurchase = productPurchase;
	}
}

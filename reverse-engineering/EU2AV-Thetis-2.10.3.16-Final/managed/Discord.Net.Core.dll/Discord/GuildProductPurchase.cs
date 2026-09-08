namespace Discord;

public readonly struct GuildProductPurchase
{
	public readonly ulong ListingId;

	public readonly string ProductName;

	internal GuildProductPurchase(ulong listingId, string productName)
	{
		ListingId = listingId;
		ProductName = productName;
	}
}

namespace Discord;

public struct GuildInventorySettings
{
	public bool IsEmojiPackCollectible { get; }

	internal GuildInventorySettings(bool isEmojiPackCollectible)
	{
		IsEmojiPackCollectible = isEmojiPackCollectible;
	}
}

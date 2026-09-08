namespace Discord;

public readonly struct PollMedia
{
	public readonly string Text;

	public readonly IEmote Emoji;

	internal PollMedia(string text, IEmote emoji)
	{
		Text = text;
		Emoji = emoji;
	}
}

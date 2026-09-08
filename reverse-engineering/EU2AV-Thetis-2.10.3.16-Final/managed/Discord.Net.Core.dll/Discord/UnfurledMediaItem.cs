namespace Discord;

public class UnfurledMediaItem
{
	public string Url { get; }

	public UnfurledMediaItemProperties ToProperties()
	{
		return new UnfurledMediaItemProperties(Url);
	}

	internal UnfurledMediaItem(string url)
	{
		Url = url;
	}
}

namespace Discord;

public struct UnfurledMediaItemProperties
{
	public string Url { get; set; }

	public UnfurledMediaItemProperties()
	{
		Url = null;
	}

	public UnfurledMediaItemProperties(string url)
	{
		Url = url;
	}

	public static implicit operator UnfurledMediaItemProperties(string url)
	{
		return new UnfurledMediaItemProperties(url);
	}
}

using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct EmbedThumbnail
{
	public string Url { get; }

	public string ProxyUrl { get; }

	public int? Height { get; }

	public int? Width { get; }

	private string DebuggerDisplay => Url + " (" + ((Width.HasValue && Height.HasValue) ? $"{Width}x{Height}" : "0x0") + ")";

	internal EmbedThumbnail(string url, string proxyUrl, int? height, int? width)
	{
		Url = url;
		ProxyUrl = proxyUrl;
		Height = height;
		Width = width;
	}

	public override string ToString()
	{
		return Url;
	}

	public static bool operator ==(EmbedThumbnail? left, EmbedThumbnail? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedThumbnail? left, EmbedThumbnail? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedThumbnail value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedThumbnail? embedThumbnail)
	{
		return GetHashCode() == embedThumbnail?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((object)(Width, Height, Url, ProxyUrl)/*cast due to constrained. prefix*/).GetHashCode();
	}
}

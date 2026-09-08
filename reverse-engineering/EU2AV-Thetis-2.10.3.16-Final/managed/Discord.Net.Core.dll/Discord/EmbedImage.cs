using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct EmbedImage
{
	public string Url { get; }

	public string ProxyUrl { get; }

	public int? Height { get; }

	public int? Width { get; }

	private string DebuggerDisplay => Url + " (" + ((Width.HasValue && Height.HasValue) ? $"{Width}x{Height}" : "0x0") + ")";

	internal EmbedImage(string url, string proxyUrl, int? height, int? width)
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

	public static bool operator ==(EmbedImage? left, EmbedImage? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedImage? left, EmbedImage? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedImage value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedImage? embedImage)
	{
		return GetHashCode() == embedImage?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((object)(Height, Width, Url, ProxyUrl)/*cast due to constrained. prefix*/).GetHashCode();
	}
}

using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct EmbedVideo
{
	public string Url { get; }

	public int? Height { get; }

	public int? Width { get; }

	private string DebuggerDisplay => Url + " (" + ((Width.HasValue && Height.HasValue) ? $"{Width}x{Height}" : "0x0") + ")";

	internal EmbedVideo(string url, int? height, int? width)
	{
		Url = url;
		Height = height;
		Width = width;
	}

	public override string ToString()
	{
		return Url;
	}

	public static bool operator ==(EmbedVideo? left, EmbedVideo? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedVideo? left, EmbedVideo? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedVideo value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedVideo? embedVideo)
	{
		return GetHashCode() == embedVideo?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((object)(Width, Height, Url)/*cast due to constrained. prefix*/).GetHashCode();
	}
}

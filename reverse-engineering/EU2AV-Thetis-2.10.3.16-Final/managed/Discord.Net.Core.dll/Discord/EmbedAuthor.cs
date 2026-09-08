using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct EmbedAuthor
{
	public string Name { get; internal set; }

	public string Url { get; internal set; }

	public string IconUrl { get; internal set; }

	public string ProxyIconUrl { get; internal set; }

	private string DebuggerDisplay => Name + " (" + Url + ")";

	internal EmbedAuthor(string name, string url, string iconUrl, string proxyIconUrl)
	{
		Name = name;
		Url = url;
		IconUrl = iconUrl;
		ProxyIconUrl = proxyIconUrl;
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(EmbedAuthor? left, EmbedAuthor? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedAuthor? left, EmbedAuthor? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedAuthor value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedAuthor? embedAuthor)
	{
		return GetHashCode() == embedAuthor?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((object)(Name, Url, IconUrl)/*cast due to constrained. prefix*/).GetHashCode();
	}
}

using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct EmbedFooter
{
	public string Text { get; }

	public string IconUrl { get; }

	public string ProxyUrl { get; }

	private string DebuggerDisplay => Text + " (" + IconUrl + ")";

	internal EmbedFooter(string text, string iconUrl, string proxyUrl)
	{
		Text = text;
		IconUrl = iconUrl;
		ProxyUrl = proxyUrl;
	}

	public override string ToString()
	{
		return Text;
	}

	public static bool operator ==(EmbedFooter? left, EmbedFooter? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedFooter? left, EmbedFooter? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedFooter value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedFooter? embedFooter)
	{
		return GetHashCode() == embedFooter?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((object)(Text, IconUrl, ProxyUrl)/*cast due to constrained. prefix*/).GetHashCode();
	}
}

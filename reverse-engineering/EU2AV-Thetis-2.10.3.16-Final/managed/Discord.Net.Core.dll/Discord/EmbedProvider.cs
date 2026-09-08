using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct EmbedProvider
{
	public string Name { get; }

	public string Url { get; }

	private string DebuggerDisplay => Name + " (" + Url + ")";

	internal EmbedProvider(string name, string url)
	{
		Name = name;
		Url = url;
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(EmbedProvider? left, EmbedProvider? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedProvider? left, EmbedProvider? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedProvider value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedProvider? embedProvider)
	{
		return GetHashCode() == embedProvider?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((object)(Name, Url)/*cast due to constrained. prefix*/).GetHashCode();
	}
}

using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct EmbedField
{
	public string Name { get; internal set; }

	public string Value { get; internal set; }

	public bool Inline { get; internal set; }

	private string DebuggerDisplay => Name + " (" + Value;

	internal EmbedField(string name, string value, bool inline)
	{
		Name = name;
		Value = value;
		Inline = inline;
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(EmbedField? left, EmbedField? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedField? left, EmbedField? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedField value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedField? embedField)
	{
		return GetHashCode() == embedField?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((object)(Name, Value, Inline)/*cast due to constrained. prefix*/).GetHashCode();
	}
}

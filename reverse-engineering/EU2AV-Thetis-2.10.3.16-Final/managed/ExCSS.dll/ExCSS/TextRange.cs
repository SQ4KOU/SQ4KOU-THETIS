using System;

namespace ExCSS;

public struct TextRange(TextPosition start, TextPosition end) : IEquatable<TextRange>, IComparable<TextRange>
{
	public TextPosition Start { get; } = start;

	public TextPosition End { get; } = end;

	public override string ToString()
	{
		return $"({Start}) -- ({End})";
	}

	public override int GetHashCode()
	{
		return End.GetHashCode() ^ Start.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is TextRange other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(TextRange other)
	{
		if (Start.Equals(other.Start))
		{
			return End.Equals(other.End);
		}
		return false;
	}

	public static bool operator >(TextRange a, TextRange b)
	{
		return a.Start > b.End;
	}

	public static bool operator <(TextRange a, TextRange b)
	{
		return a.End < b.Start;
	}

	public int CompareTo(TextRange other)
	{
		if (this > other)
		{
			return 1;
		}
		if (other > this)
		{
			return -1;
		}
		return 0;
	}
}

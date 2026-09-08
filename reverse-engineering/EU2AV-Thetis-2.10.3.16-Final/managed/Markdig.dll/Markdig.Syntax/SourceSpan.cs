using System;

namespace Markdig.Syntax;

public struct SourceSpan(int start, int end) : IEquatable<SourceSpan>
{
	public static readonly SourceSpan Empty = new SourceSpan(0, -1);

	public int Start { get; set; } = start;

	public int End { get; set; } = end;

	public readonly int Length => End - Start + 1;

	public readonly bool IsEmpty => Start > End;

	public SourceSpan MoveForward(int count)
	{
		return new SourceSpan(Start + count, End + count);
	}

	public readonly bool Equals(SourceSpan other)
	{
		if (Start == other.Start)
		{
			return End == other.End;
		}
		return false;
	}

	public override readonly bool Equals(object? obj)
	{
		if (obj is SourceSpan other)
		{
			return Equals(other);
		}
		return false;
	}

	public override readonly int GetHashCode()
	{
		return (Start * 397) ^ End;
	}

	public static bool operator ==(SourceSpan left, SourceSpan right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SourceSpan left, SourceSpan right)
	{
		return !left.Equals(right);
	}

	public override readonly string ToString()
	{
		return $"{Start}-{End}";
	}
}

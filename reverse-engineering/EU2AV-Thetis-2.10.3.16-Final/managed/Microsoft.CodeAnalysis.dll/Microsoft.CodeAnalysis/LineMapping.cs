using System;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct LineMapping : IEquatable<LineMapping>
{
	public LinePositionSpan Span { get; }

	public int? CharacterOffset { get; }

	public FileLinePositionSpan MappedSpan { get; }

	public bool IsHidden => !MappedSpan.IsValid;

	public LineMapping(LinePositionSpan span, int? characterOffset, FileLinePositionSpan mappedSpan)
	{
		Span = span;
		CharacterOffset = characterOffset;
		MappedSpan = mappedSpan;
	}

	public override bool Equals(object? obj)
	{
		if (obj is LineMapping other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(LineMapping other)
	{
		if (Span.Equals(other.Span) && CharacterOffset.Equals(other.CharacterOffset))
		{
			return MappedSpan.Equals(other.MappedSpan);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Hash.Combine(Span.GetHashCode(), CharacterOffset.GetHashCode()), MappedSpan.GetHashCode());
	}

	public static bool operator ==(LineMapping left, LineMapping right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LineMapping left, LineMapping right)
	{
		return !(left == right);
	}

	public override string? ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Span);
		if (CharacterOffset.HasValue)
		{
			stringBuilder.Append(',');
			stringBuilder.Append(CharacterOffset.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
		}
		stringBuilder.Append(" -> ");
		stringBuilder.Append(MappedSpan);
		return stringBuilder.ToString();
	}
}

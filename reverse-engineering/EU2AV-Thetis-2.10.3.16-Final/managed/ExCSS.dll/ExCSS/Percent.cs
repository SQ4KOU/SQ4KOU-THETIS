using System;

namespace ExCSS;

public struct Percent(float value) : IEquatable<Percent>, IComparable<Percent>, IFormattable
{
	public static readonly Percent Zero = new Percent(0f);

	public static readonly Percent Fifty = new Percent(50f);

	public static readonly Percent Hundred = new Percent(100f);

	public float NormalizedValue => Value * 0.01f;

	public float Value { get; } = value;

	public static bool operator >=(Percent a, Percent b)
	{
		return a.Value >= b.Value;
	}

	public static bool operator >(Percent a, Percent b)
	{
		return a.Value > b.Value;
	}

	public static bool operator <=(Percent a, Percent b)
	{
		return a.Value <= b.Value;
	}

	public static bool operator <(Percent a, Percent b)
	{
		return a.Value < b.Value;
	}

	public int CompareTo(Percent other)
	{
		return Value.CompareTo(other.Value);
	}

	public bool Equals(Percent other)
	{
		return Value == other.Value;
	}

	public static bool operator ==(Percent a, Percent b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Percent a, Percent b)
	{
		return !a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		if (obj is Percent other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override string ToString()
	{
		return Value + "%";
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return Value.ToString(format, formatProvider) + "%";
	}
}

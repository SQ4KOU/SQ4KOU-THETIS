using System;

namespace ExCSS;

public struct Number(float value, Number.Unit unit) : IEquatable<Number>, IComparable<Number>, IFormattable
{
	public enum Unit : byte
	{
		Integer,
		Float,
		Percent
	}

	public static readonly Number Zero = new Number(0f, Unit.Integer);

	public static readonly Number Infinite = new Number(float.PositiveInfinity, Unit.Float);

	public static readonly Number One = new Number(1f, Unit.Integer);

	private readonly Unit _unit = unit;

	public float Value { get; } = value;

	public bool IsInteger => _unit == Unit.Integer;

	public static bool operator >=(Number a, Number b)
	{
		return a.Value >= b.Value;
	}

	public static bool operator >(Number a, Number b)
	{
		return a.Value > b.Value;
	}

	public static bool operator <=(Number a, Number b)
	{
		return a.Value <= b.Value;
	}

	public static bool operator <(Number a, Number b)
	{
		return a.Value < b.Value;
	}

	public int CompareTo(Number other)
	{
		return Value.CompareTo(other.Value);
	}

	public bool Equals(Number other)
	{
		if (Value == other.Value)
		{
			return _unit == other._unit;
		}
		return false;
	}

	public static bool operator ==(Number a, Number b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(Number a, Number b)
	{
		return a.Value != b.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is Number other)
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
		return Value + ((_unit == Unit.Percent) ? "%" : string.Empty);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return Value.ToString(format, formatProvider) + ((_unit == Unit.Percent) ? "%" : string.Empty);
	}
}

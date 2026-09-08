using System;

namespace ExCSS;

public struct Time(float value, Time.Unit unit) : IEquatable<Time>, IComparable<Time>, IFormattable
{
	public enum Unit : byte
	{
		None,
		Ms,
		S
	}

	public static readonly Time Zero = new Time(0f, Unit.Ms);

	public float Value { get; } = value;

	public Unit Type { get; } = unit;

	public string UnitString => Type switch
	{
		Unit.Ms => UnitNames.Ms, 
		Unit.S => UnitNames.S, 
		_ => string.Empty, 
	};

	public static bool operator >=(Time a, Time b)
	{
		int num = a.CompareTo(b);
		if (num != 0)
		{
			return num == 1;
		}
		return true;
	}

	public static bool operator >(Time a, Time b)
	{
		return a.CompareTo(b) == 1;
	}

	public static bool operator <=(Time a, Time b)
	{
		int num = a.CompareTo(b);
		if (num != 0)
		{
			return num == -1;
		}
		return true;
	}

	public static bool operator <(Time a, Time b)
	{
		return a.CompareTo(b) == -1;
	}

	public int CompareTo(Time other)
	{
		return ToMilliseconds().CompareTo(other.ToMilliseconds());
	}

	public static Unit GetUnit(string s)
	{
		if (!(s == "s"))
		{
			if (s == "ms")
			{
				return Unit.Ms;
			}
			return Unit.None;
		}
		return Unit.S;
	}

	public float ToMilliseconds()
	{
		if (Type != Unit.S)
		{
			return Value;
		}
		return Value * 1000f;
	}

	public bool Equals(Time other)
	{
		return ToMilliseconds() == other.ToMilliseconds();
	}

	public static bool operator ==(Time a, Time b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Time a, Time b)
	{
		return !a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		if (obj is Time other)
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
		return Value + UnitString;
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return Value.ToString(format, formatProvider) + UnitString;
	}
}

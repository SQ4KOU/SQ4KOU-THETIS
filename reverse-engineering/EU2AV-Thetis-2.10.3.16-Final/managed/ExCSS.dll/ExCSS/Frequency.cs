using System;

namespace ExCSS;

public struct Frequency(float value, Frequency.Unit unit) : IEquatable<Frequency>, IComparable<Frequency>, IFormattable
{
	public enum Unit : byte
	{
		None,
		Hz,
		Khz
	}

	public float Value { get; } = value;

	public Unit Type { get; } = unit;

	public string UnitString => Type switch
	{
		Unit.Khz => UnitNames.Khz, 
		Unit.Hz => UnitNames.Hz, 
		_ => string.Empty, 
	};

	public static bool operator >=(Frequency a, Frequency b)
	{
		int num = a.CompareTo(b);
		if (num != 0)
		{
			return num == 1;
		}
		return true;
	}

	public static bool operator >(Frequency a, Frequency b)
	{
		return a.CompareTo(b) == 1;
	}

	public static bool operator <=(Frequency a, Frequency b)
	{
		int num = a.CompareTo(b);
		if (num != 0)
		{
			return num == -1;
		}
		return true;
	}

	public static bool operator <(Frequency a, Frequency b)
	{
		return a.CompareTo(b) == -1;
	}

	public int CompareTo(Frequency other)
	{
		return ToHertz().CompareTo(other.ToHertz());
	}

	public static bool TryParse(string s, out Frequency result)
	{
		Unit unit = GetUnit(s.StylesheetUnit(out var result2));
		if (unit != Unit.None)
		{
			result = new Frequency(result2, unit);
			return true;
		}
		result = default(Frequency);
		return false;
	}

	public static Unit GetUnit(string s)
	{
		if (!(s == "hz"))
		{
			if (s == "khz")
			{
				return Unit.Khz;
			}
			return Unit.None;
		}
		return Unit.Hz;
	}

	public float ToHertz()
	{
		if (Type != Unit.Khz)
		{
			return Value;
		}
		return Value * 1000f;
	}

	public bool Equals(Frequency other)
	{
		if (Value == other.Value)
		{
			return Type == other.Type;
		}
		return false;
	}

	public static bool operator ==(Frequency a, Frequency b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Frequency a, Frequency b)
	{
		return !a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		Frequency? frequency = obj as Frequency?;
		if (frequency.HasValue)
		{
			return Equals(frequency.Value);
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

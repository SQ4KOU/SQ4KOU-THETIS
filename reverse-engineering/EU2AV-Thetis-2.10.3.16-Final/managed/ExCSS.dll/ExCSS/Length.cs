using System;

namespace ExCSS;

public struct Length(float value, Length.Unit unit) : IEquatable<Length>, IComparable<Length>, IFormattable
{
	public enum Unit : byte
	{
		None,
		Px,
		Em,
		Ex,
		Cm,
		Mm,
		In,
		Pt,
		Pc,
		Ch,
		Rem,
		Vw,
		Vh,
		Vmin,
		Vmax,
		Percent
	}

	public static readonly Length Zero = new Length(0f, Unit.Px);

	public static readonly Length Half = new Length(50f, Unit.Percent);

	public static readonly Length Full = new Length(100f, Unit.Percent);

	public static readonly Length Thin = new Length(1f, Unit.Px);

	public static readonly Length Medium = new Length(3f, Unit.Px);

	public static readonly Length Thick = new Length(5f, Unit.Px);

	public static readonly Length Missing = new Length(-1f, Unit.Ch);

	public bool IsAbsolute
	{
		get
		{
			if (Type != Unit.In && Type != Unit.Mm && Type != Unit.Pc && Type != Unit.Px && Type != Unit.Pt)
			{
				return Type == Unit.Cm;
			}
			return true;
		}
	}

	public bool IsRelative => !IsAbsolute;

	public Unit Type { get; } = unit;

	public float Value { get; } = value;

	public string UnitString => Type switch
	{
		Unit.Px => UnitNames.Px, 
		Unit.Em => UnitNames.Em, 
		Unit.Ex => UnitNames.Ex, 
		Unit.Cm => UnitNames.Cm, 
		Unit.Mm => UnitNames.Mm, 
		Unit.In => UnitNames.In, 
		Unit.Pt => UnitNames.Pt, 
		Unit.Pc => UnitNames.Pc, 
		Unit.Ch => UnitNames.Ch, 
		Unit.Rem => UnitNames.Rem, 
		Unit.Vw => UnitNames.Vw, 
		Unit.Vh => UnitNames.Vh, 
		Unit.Vmin => UnitNames.Vmin, 
		Unit.Vmax => UnitNames.Vmax, 
		Unit.Percent => UnitNames.Percent, 
		_ => string.Empty, 
	};

	public static bool operator >=(Length a, Length b)
	{
		int num = a.CompareTo(b);
		if (num != 0)
		{
			return num == 1;
		}
		return true;
	}

	public static bool operator >(Length a, Length b)
	{
		return a.CompareTo(b) == 1;
	}

	public static bool operator <=(Length a, Length b)
	{
		int num = a.CompareTo(b);
		if (num != 0)
		{
			return num == -1;
		}
		return true;
	}

	public static bool operator <(Length a, Length b)
	{
		return a.CompareTo(b) == -1;
	}

	public int CompareTo(Length other)
	{
		if (Type == other.Type)
		{
			return Value.CompareTo(other.Value);
		}
		if (IsAbsolute && other.IsAbsolute)
		{
			return ToPixel().CompareTo(other.ToPixel());
		}
		return 0;
	}

	public static bool TryParse(string s, out Length result)
	{
		Unit unit = GetUnit(s.StylesheetUnit(out var result2));
		if (unit != Unit.None)
		{
			result = new Length(result2, unit);
			return true;
		}
		if (result2 == 0f)
		{
			result = Zero;
			return true;
		}
		result = default(Length);
		return false;
	}

	public static Unit GetUnit(string s)
	{
		return s switch
		{
			"ch" => Unit.Ch, 
			"cm" => Unit.Cm, 
			"em" => Unit.Em, 
			"ex" => Unit.Ex, 
			"in" => Unit.In, 
			"mm" => Unit.Mm, 
			"pc" => Unit.Pc, 
			"pt" => Unit.Pt, 
			"px" => Unit.Px, 
			"rem" => Unit.Rem, 
			"vh" => Unit.Vh, 
			"vmax" => Unit.Vmax, 
			"vmin" => Unit.Vmin, 
			"vw" => Unit.Vw, 
			"%" => Unit.Percent, 
			_ => Unit.None, 
		};
	}

	public float ToPixel()
	{
		return Type switch
		{
			Unit.In => Value * 96f, 
			Unit.Mm => Value * 5f * 96f / 127f, 
			Unit.Pc => Value * 12f * 96f / 72f, 
			Unit.Pt => Value * 96f / 72f, 
			Unit.Cm => Value * 50f * 96f / 127f, 
			Unit.Px => Value, 
			_ => throw new InvalidOperationException("A relative unit cannot be converted."), 
		};
	}

	public float To(Unit unit)
	{
		float num = ToPixel();
		return unit switch
		{
			Unit.In => num / 96f, 
			Unit.Mm => num * 127f / 480f, 
			Unit.Pc => num * 72f / 1152f, 
			Unit.Pt => num * 72f / 96f, 
			Unit.Cm => num * 127f / 4800f, 
			Unit.Px => num, 
			_ => throw new InvalidOperationException("An absolute unit cannot be converted to a relative one."), 
		};
	}

	public bool Equals(Length other)
	{
		if (Value == other.Value)
		{
			return Type == other.Type;
		}
		return false;
	}

	public static bool operator ==(Length a, Length b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Length a, Length b)
	{
		return !a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		Length? length = obj as Length?;
		if (length.HasValue)
		{
			return Equals(length.Value);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override string ToString()
	{
		string text = ((Value == 0f) ? string.Empty : UnitString);
		return Value + text;
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string text = ((Value == 0f) ? string.Empty : UnitString);
		return Value.ToString(format, formatProvider) + text;
	}
}

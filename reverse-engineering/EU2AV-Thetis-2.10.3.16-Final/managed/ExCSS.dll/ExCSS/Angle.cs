using System;

namespace ExCSS;

public struct Angle(float value, Angle.Unit unit) : IEquatable<Angle>, IComparable<Angle>, IFormattable
{
	public enum Unit : byte
	{
		None,
		Deg,
		Rad,
		Grad,
		Turn
	}

	public static readonly Angle Zero = new Angle(0f, Unit.Rad);

	public static readonly Angle HalfQuarter = new Angle(45f, Unit.Deg);

	public static readonly Angle Quarter = new Angle(90f, Unit.Deg);

	public static readonly Angle TripleHalfQuarter = new Angle(135f, Unit.Deg);

	public static readonly Angle Half = new Angle(180f, Unit.Deg);

	public float Value { get; } = value;

	public Unit Type { get; } = unit;

	public string UnitString => Type switch
	{
		Unit.Deg => UnitNames.Deg, 
		Unit.Grad => UnitNames.Grad, 
		Unit.Turn => UnitNames.Turn, 
		Unit.Rad => UnitNames.Rad, 
		_ => string.Empty, 
	};

	public static bool operator >=(Angle a, Angle b)
	{
		int num = a.CompareTo(b);
		if ((uint)num <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool operator >(Angle a, Angle b)
	{
		return a.CompareTo(b) == 1;
	}

	public static bool operator <=(Angle a, Angle b)
	{
		int num = a.CompareTo(b);
		if ((uint)(num - -1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool operator <(Angle a, Angle b)
	{
		return a.CompareTo(b) == -1;
	}

	public int CompareTo(Angle other)
	{
		return ToRadian().CompareTo(other.ToRadian());
	}

	public static bool TryParse(string s, out Angle result)
	{
		Unit unit = GetUnit(s.StylesheetUnit(out var result2));
		if (unit != Unit.None)
		{
			result = new Angle(result2, unit);
			return true;
		}
		result = default(Angle);
		return false;
	}

	public static Unit GetUnit(string s)
	{
		return s switch
		{
			"deg" => Unit.Deg, 
			"grad" => Unit.Grad, 
			"turn" => Unit.Turn, 
			"rad" => Unit.Rad, 
			_ => Unit.None, 
		};
	}

	public float ToRadian()
	{
		return Type switch
		{
			Unit.Deg => (float)(Math.PI / 180.0 * (double)Value), 
			Unit.Grad => (float)(Math.PI / 200.0 * (double)Value), 
			Unit.Turn => (float)(Math.PI * 2.0 * (double)Value), 
			_ => Value, 
		};
	}

	public float ToTurns()
	{
		return Type switch
		{
			Unit.Deg => (float)((double)Value / 360.0), 
			Unit.Grad => (float)((double)Value / 400.0), 
			Unit.Rad => (float)((double)Value / (Math.PI * 2.0)), 
			_ => Value, 
		};
	}

	public bool Equals(Angle other)
	{
		return ToRadian() == other.ToRadian();
	}

	public static bool operator ==(Angle a, Angle b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Angle a, Angle b)
	{
		return !a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		Angle? angle = obj as Angle?;
		if (angle.HasValue)
		{
			return Equals(angle.Value);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)Value;
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

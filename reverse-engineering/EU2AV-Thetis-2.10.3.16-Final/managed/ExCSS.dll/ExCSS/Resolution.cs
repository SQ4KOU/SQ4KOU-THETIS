using System;

namespace ExCSS;

public struct Resolution(float value, Resolution.Unit unit) : IEquatable<Resolution>, IComparable<Resolution>, IFormattable
{
	public enum Unit : byte
	{
		None,
		Dpi,
		Dpcm,
		Dppx
	}

	public float Value { get; } = value;

	public Unit Type { get; } = unit;

	public string UnitString => Type switch
	{
		Unit.Dpcm => UnitNames.Dpcm, 
		Unit.Dpi => UnitNames.Dpi, 
		Unit.Dppx => UnitNames.Dppx, 
		_ => string.Empty, 
	};

	public static bool TryParse(string s, out Resolution result)
	{
		Unit unit = GetUnit(s.StylesheetUnit(out var result2));
		if (unit != Unit.None)
		{
			result = new Resolution(result2, unit);
			return true;
		}
		result = default(Resolution);
		return false;
	}

	public static Unit GetUnit(string s)
	{
		return s switch
		{
			"dpcm" => Unit.Dpcm, 
			"dpi" => Unit.Dpi, 
			"dppx" => Unit.Dppx, 
			_ => Unit.None, 
		};
	}

	public float ToDotsPerPixel()
	{
		if (Type == Unit.Dpi)
		{
			return Value / 96f;
		}
		if (Type == Unit.Dpcm)
		{
			return Value * 127f / 4800f;
		}
		return Value;
	}

	public float To(Unit unit)
	{
		float num = ToDotsPerPixel();
		return unit switch
		{
			Unit.Dpi => num * 96f, 
			Unit.Dpcm => num * 50f * 96f / 127f, 
			_ => num, 
		};
	}

	public bool Equals(Resolution other)
	{
		if (Value == other.Value)
		{
			return Type == other.Type;
		}
		return false;
	}

	public int CompareTo(Resolution other)
	{
		return ToDotsPerPixel().CompareTo(other.ToDotsPerPixel());
	}

	public override bool Equals(object obj)
	{
		if (obj is Resolution other)
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

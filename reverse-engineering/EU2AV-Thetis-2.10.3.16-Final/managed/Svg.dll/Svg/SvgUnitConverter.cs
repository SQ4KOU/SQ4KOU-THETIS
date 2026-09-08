using System;
using System.ComponentModel;
using System.Globalization;
using Svg.Helpers;

namespace Svg;

public sealed class SvgUnitConverter : TypeConverter
{
	public static SvgUnit Parse(ReadOnlySpan<char> unit)
	{
		int num = -1;
		switch (unit)
		{
		case "none":
			return SvgUnit.None;
		case "medium":
			return new SvgUnit(SvgUnitType.Em, 1f);
		case "small":
			return new SvgUnit(SvgUnitType.Em, 0.8f);
		case "x-small":
			return new SvgUnit(SvgUnitType.Em, 0.7f);
		case "xx-small":
			return new SvgUnit(SvgUnitType.Em, 0.6f);
		case "large":
			return new SvgUnit(SvgUnitType.Em, 1.2f);
		case "x-large":
			return new SvgUnit(SvgUnitType.Em, 1.4f);
		case "xx-large":
			return new SvgUnit(SvgUnitType.Em, 1.7f);
		default:
		{
			int length = unit.Length;
			for (int i = 0; i < length; i++)
			{
				char c = unit[i];
				if (c == '%')
				{
					num = i;
					break;
				}
				if (char.IsLetter(c) && ((c != 'e' && c != 'E') || i >= length - 1 || char.IsLetter(unit[i + 1])))
				{
					num = i;
					break;
				}
			}
			float value = StringParser.ToFloat((num > -1) ? unit.Slice(0, num) : unit);
			if (num == -1)
			{
				return new SvgUnit(value);
			}
			Span<char> destination = stackalloc char[2];
			switch (unit.Slice(num).Trim().ToLowerInvariant(destination))
			{
			default:
				throw new FormatException("Unit is in an invalid format '" + unit.ToString() + "'.");
			case 1:
				if (destination[0] == '%')
				{
					return new SvgUnit(SvgUnitType.Percentage, value);
				}
				break;
			case 2:
			{
				char c2 = destination[0];
				char c3 = destination[1];
				if (c2 == 'm' && c3 == 'm')
				{
					return new SvgUnit(SvgUnitType.Millimeter, value);
				}
				if (c2 == 'c' && c3 == 'm')
				{
					return new SvgUnit(SvgUnitType.Centimeter, value);
				}
				if (c2 == 'i' && c3 == 'n')
				{
					return new SvgUnit(SvgUnitType.Inch, value);
				}
				if (c2 == 'p' && c3 == 'x')
				{
					return new SvgUnit(SvgUnitType.Pixel, value);
				}
				if (c2 == 'p' && c3 == 't')
				{
					return new SvgUnit(SvgUnitType.Point, value);
				}
				if (c2 == 'p' && c3 == 'c')
				{
					return new SvgUnit(SvgUnitType.Pica, value);
				}
				if (c2 == 'e' && c3 == 'm')
				{
					return new SvgUnit(SvgUnitType.Em, value);
				}
				if (c2 == 'e' && c3 == 'x')
				{
					return new SvgUnit(SvgUnitType.Ex, value);
				}
				break;
			}
			}
			throw new FormatException("Unit is in an invalid format '" + unit.ToString() + "'.");
		}
		}
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			return new SvgUnit(SvgUnitType.User, 0f);
		}
		return Parse(((value as string) ?? throw new ArgumentException("The value argument must be a string.")).AsSpan());
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!(sourceType == typeof(string)))
		{
			return base.CanConvertFrom(context, sourceType);
		}
		return true;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(string)))
		{
			return base.CanConvertTo(context, destinationType);
		}
		return true;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!(destinationType == typeof(string)))
		{
			return base.ConvertTo(context, culture, value, destinationType);
		}
		return ((SvgUnit)value/*cast due to constrained. prefix*/).ToString();
	}
}

using System;
using System.ComponentModel;
using System.Globalization;

namespace Svg;

public sealed class SvgFontWeightConverter : EnumBaseConverter<SvgFontWeight>
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			switch ((string)value)
			{
			case "100":
				return SvgFontWeight.W100;
			case "200":
				return SvgFontWeight.W200;
			case "300":
				return SvgFontWeight.W300;
			case "400":
				return SvgFontWeight.W400;
			case "500":
				return SvgFontWeight.W500;
			case "600":
				return SvgFontWeight.W600;
			case "700":
				return SvgFontWeight.W700;
			case "800":
				return SvgFontWeight.W800;
			case "900":
				return SvgFontWeight.W900;
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is SvgFontWeight svgFontWeight)
		{
			switch (svgFontWeight)
			{
			case SvgFontWeight.W100:
				return "100";
			case SvgFontWeight.W200:
				return "200";
			case SvgFontWeight.W300:
				return "300";
			case SvgFontWeight.W400:
				return "400";
			case SvgFontWeight.W500:
				return "500";
			case SvgFontWeight.W600:
				return "600";
			case SvgFontWeight.W700:
				return "700";
			case SvgFontWeight.W800:
				return "800";
			case SvgFontWeight.W900:
				return "900";
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public SvgFontWeightConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

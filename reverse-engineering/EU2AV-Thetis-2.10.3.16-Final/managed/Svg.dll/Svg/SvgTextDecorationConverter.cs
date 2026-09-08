using System;
using System.ComponentModel;
using System.Globalization;

namespace Svg;

public sealed class SvgTextDecorationConverter : EnumBaseConverter<SvgTextDecoration>
{
	public SvgTextDecorationConverter()
		: base(CaseHandling.KebabCase)
	{
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			value = ((string)value).Replace(" ", ", ");
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		object obj = base.ConvertTo(context, culture, value, destinationType);
		if (obj is string && value is SvgTextDecoration)
		{
			obj = ((string)obj).Replace(",", string.Empty);
		}
		return obj;
	}
}

using System;
using System.ComponentModel;
using System.Globalization;

namespace Svg;

internal class SvgStrokeDashArrayConverter : SvgUnitCollectionConverter
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			ReadOnlySpan<char> readOnlySpan = text.AsSpan();
			ReadOnlySpan<char> span = readOnlySpan.Trim();
			if (span.Equals("none".AsSpan(), StringComparison.OrdinalIgnoreCase))
			{
				return new SvgUnitCollection
				{
					StringForEmptyValue = "none"
				};
			}
			if (span.Equals("inherit".AsSpan(), StringComparison.OrdinalIgnoreCase))
			{
				return new SvgUnitCollection
				{
					StringForEmptyValue = "inherit"
				};
			}
			return SvgUnitCollectionConverter.Parse(readOnlySpan);
		}
		return base.ConvertFrom(context, culture, value);
	}
}

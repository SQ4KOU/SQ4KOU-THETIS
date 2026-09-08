using System;
using System.ComponentModel;
using System.Globalization;

namespace Svg;

internal class SvgPointCollectionConverter : TypeConverter
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			ReadOnlySpan<char> chars = text.AsSpan().Trim();
			CoordinateParserState state = new CoordinateParserState(ref chars);
			SvgPointCollection svgPointCollection = new SvgPointCollection();
			float result;
			while (CoordinateParser.TryGetFloat(out result, chars, ref state))
			{
				svgPointCollection.Add(new SvgUnit(SvgUnitType.User, result));
			}
			return svgPointCollection;
		}
		return base.ConvertFrom(context, culture, value);
	}
}

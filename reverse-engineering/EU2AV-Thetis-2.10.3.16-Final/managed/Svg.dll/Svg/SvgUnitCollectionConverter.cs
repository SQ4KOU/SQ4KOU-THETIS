using System;
using System.ComponentModel;
using System.Globalization;
using Svg.Helpers;

namespace Svg;

public class SvgUnitCollectionConverter : TypeConverter
{
	private static readonly char[] SplitChars = new char[5] { ',', ' ', '\r', '\n', '\t' };

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			return Parse(text.AsSpan());
		}
		return base.ConvertFrom(context, culture, value);
	}

	public static SvgUnitCollection Parse(ReadOnlySpan<char> points)
	{
		SvgUnitCollection svgUnitCollection = new SvgUnitCollection();
		Span<char> span = SplitChars.AsSpan();
		StringSplitEnumerator enumerator = new StringSplitEnumerator(points, span).GetEnumerator();
		while (enumerator.MoveNext())
		{
			SvgUnit item = SvgUnitConverter.Parse(enumerator.Current.Value);
			if (!item.IsNone)
			{
				svgUnitCollection.Add(item);
			}
		}
		return svgUnitCollection;
	}
}

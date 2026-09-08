using System;
using System.ComponentModel;
using System.Globalization;
using Svg.Helpers;

namespace Svg;

public class SvgNumberCollectionConverter : TypeConverter
{
	private static readonly char[] SplitChars = new char[5] { ' ', '\t', '\n', '\r', ',' };

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			return Parse(text.AsSpan());
		}
		return base.ConvertFrom(context, culture, value);
	}

	public static SvgNumberCollection Parse(ReadOnlySpan<char> numbers)
	{
		SvgNumberCollection svgNumberCollection = new SvgNumberCollection();
		Span<char> span = SplitChars.AsSpan();
		StringSplitEnumerator enumerator = new StringSplitEnumerator(numbers, span).GetEnumerator();
		while (enumerator.MoveNext())
		{
			float item = StringParser.ToFloatAny(enumerator.Current.Value);
			svgNumberCollection.Add(item);
		}
		return svgNumberCollection;
	}
}

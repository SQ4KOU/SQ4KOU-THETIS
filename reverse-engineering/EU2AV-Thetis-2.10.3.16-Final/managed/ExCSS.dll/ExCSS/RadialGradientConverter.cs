using System.Collections.Generic;

namespace ExCSS;

internal sealed class RadialGradientConverter : GradientConverter
{
	private readonly IValueConverter _converter;

	public RadialGradientConverter()
	{
		IValueConverter valueConverter = Converters.PointConverter.StartsWithKeyword(Keywords.At).Option(Point.Center);
		IValueConverter primary = Converters.WithOrder(Converters.WithAny(Converters.Assign(Keywords.Circle, result: true).Option(defaultValue: true), Converters.LengthConverter.Option()), valueConverter);
		IValueConverter primary2 = Converters.WithOrder(Converters.WithAny(Converters.Assign(Keywords.Ellipse, result: false).Option(defaultValue: false), Converters.LengthOrPercentConverter.Many(2, 2).Option()), valueConverter);
		IValueConverter secondary = Converters.WithOrder(Converters.WithAny(Converters.Toggle(Keywords.Circle, Keywords.Ellipse).Option(defaultValue: false), Map.RadialGradientSizeModes.ToConverter()), valueConverter);
		_converter = primary.Or(primary2.Or(secondary));
	}

	protected override IPropertyValue ConvertFirstArgument(IEnumerable<Token> value)
	{
		return _converter.Convert(value);
	}
}

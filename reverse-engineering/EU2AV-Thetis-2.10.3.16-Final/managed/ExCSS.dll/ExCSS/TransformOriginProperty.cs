namespace ExCSS;

internal sealed class TransformOriginProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.WithOrder(Converters.LengthOrPercentConverter.Or(Keywords.Center, Point.Center).Or(Converters.WithAny(Converters.LengthOrPercentConverter.Or(Keywords.Left, Length.Zero).Or(Keywords.Right, Length.Full).Or(Keywords.Center, Length.Half)
		.Option(Length.Half), Converters.LengthOrPercentConverter.Or(Keywords.Top, Length.Zero).Or(Keywords.Bottom, Length.Full).Or(Keywords.Center, Length.Half)
		.Option(Length.Half))).Or(Converters.WithAny(Converters.LengthOrPercentConverter.Or(Keywords.Top, Length.Zero).Or(Keywords.Bottom, Length.Full).Or(Keywords.Center, Length.Half)
		.Option(Length.Half), Converters.LengthOrPercentConverter.Or(Keywords.Left, Length.Zero).Or(Keywords.Right, Length.Full).Or(Keywords.Center, Length.Half)
		.Option(Length.Half)))
		.Required(), Converters.LengthConverter.Option(Length.Zero)).OrDefault(Point.Center);

	internal override IValueConverter Converter => StyleConverter;

	internal TransformOriginProperty()
		: base(PropertyNames.TransformOrigin, PropertyFlags.Animatable)
	{
	}
}

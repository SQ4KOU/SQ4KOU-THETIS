namespace ExCSS;

internal sealed class PerspectiveOriginProperty : Property
{
	private static readonly IValueConverter PerspectiveConverter = Converters.LengthOrPercentConverter.Or(Keywords.Left, new Point(Length.Zero, Length.Half)).Or(Keywords.Center, new Point(Length.Half, Length.Half)).Or(Keywords.Right, new Point(Length.Full, Length.Half))
		.Or(Keywords.Top, new Point(Length.Half, Length.Zero))
		.Or(Keywords.Bottom, new Point(Length.Half, Length.Full))
		.Or(Converters.WithAny(Converters.LengthOrPercentConverter.Or(Keywords.Left, Length.Zero).Or(Keywords.Right, Length.Full).Or(Keywords.Center, Length.Half)
			.Option(Length.Half), Converters.LengthOrPercentConverter.Or(Keywords.Top, Length.Zero).Or(Keywords.Bottom, Length.Full).Or(Keywords.Center, Length.Half)
			.Option(Length.Half)))
		.OrDefault(Point.Center);

	internal override IValueConverter Converter => PerspectiveConverter;

	internal PerspectiveOriginProperty()
		: base(PropertyNames.PerspectiveOrigin, PropertyFlags.Animatable)
	{
	}
}

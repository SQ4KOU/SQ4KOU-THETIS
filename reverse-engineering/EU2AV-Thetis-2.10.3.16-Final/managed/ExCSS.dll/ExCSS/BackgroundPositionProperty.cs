namespace ExCSS;

internal sealed class BackgroundPositionProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.PointConverter.FromList().OrDefault(Point.Center);

	internal override IValueConverter Converter => ListConverter;

	internal BackgroundPositionProperty()
		: base(PropertyNames.BackgroundPosition, PropertyFlags.Animatable)
	{
	}
}

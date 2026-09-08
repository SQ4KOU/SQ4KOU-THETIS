namespace ExCSS;

internal sealed class ObjectPositionProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.PointConverter.OrDefault(Point.Center);

	internal override IValueConverter Converter => StyleConverter;

	internal ObjectPositionProperty()
		: base(PropertyNames.ObjectPosition, PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class DirectionProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.DirectionModeConverter.OrDefault(DirectionMode.Ltr);

	internal override IValueConverter Converter => StyleConverter;

	internal DirectionProperty()
		: base(PropertyNames.Direction, PropertyFlags.Inherited)
	{
	}
}

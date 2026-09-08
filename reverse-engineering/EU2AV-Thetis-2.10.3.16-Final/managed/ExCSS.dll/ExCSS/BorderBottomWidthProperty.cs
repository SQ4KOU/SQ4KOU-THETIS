namespace ExCSS;

internal sealed class BorderBottomWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderBottomWidthProperty()
		: base(PropertyNames.BorderBottomWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

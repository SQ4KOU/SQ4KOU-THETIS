namespace ExCSS;

internal sealed class BorderTopWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderTopWidthProperty()
		: base(PropertyNames.BorderTopWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

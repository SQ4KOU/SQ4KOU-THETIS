namespace ExCSS;

internal sealed class BorderLeftWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderLeftWidthProperty()
		: base(PropertyNames.BorderLeftWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

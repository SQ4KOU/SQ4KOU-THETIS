namespace ExCSS;

internal sealed class BorderTopLeftRadiusProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BorderRadiusConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderTopLeftRadiusProperty()
		: base(PropertyNames.BorderTopLeftRadius, PropertyFlags.Animatable)
	{
	}
}

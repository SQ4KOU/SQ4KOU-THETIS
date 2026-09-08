namespace ExCSS;

internal sealed class BorderBottomLeftRadiusProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BorderRadiusConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderBottomLeftRadiusProperty()
		: base(PropertyNames.BorderBottomLeftRadius, PropertyFlags.Animatable)
	{
	}
}

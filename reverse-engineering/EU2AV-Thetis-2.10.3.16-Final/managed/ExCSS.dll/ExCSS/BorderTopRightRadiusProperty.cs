namespace ExCSS;

internal sealed class BorderTopRightRadiusProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BorderRadiusConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderTopRightRadiusProperty()
		: base(PropertyNames.BorderTopRightRadius, PropertyFlags.Animatable)
	{
	}
}

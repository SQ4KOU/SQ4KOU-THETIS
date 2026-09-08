namespace ExCSS;

internal sealed class BorderRadiusProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.BorderRadiusShorthandConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderRadiusProperty()
		: base(PropertyNames.BorderRadius, PropertyFlags.Animatable)
	{
	}
}

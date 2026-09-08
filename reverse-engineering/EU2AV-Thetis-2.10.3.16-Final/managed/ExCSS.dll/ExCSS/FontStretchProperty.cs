namespace ExCSS;

internal sealed class FontStretchProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FontStretchConverter.OrDefault(FontStretch.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal FontStretchProperty()
		: base(PropertyNames.FontStretch, PropertyFlags.Inherited | PropertyFlags.Animatable)
	{
	}
}

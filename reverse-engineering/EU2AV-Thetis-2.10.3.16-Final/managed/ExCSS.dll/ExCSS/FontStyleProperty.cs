namespace ExCSS;

internal sealed class FontStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FontStyleConverter.OrDefault(FontStyle.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal FontStyleProperty()
		: base(PropertyNames.FontStyle, PropertyFlags.Inherited)
	{
	}
}

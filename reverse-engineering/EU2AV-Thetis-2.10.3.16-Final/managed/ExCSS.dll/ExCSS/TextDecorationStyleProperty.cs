namespace ExCSS;

internal sealed class TextDecorationStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.TextDecorationStyleConverter.OrDefault(TextDecorationStyle.Solid);

	internal override IValueConverter Converter => StyleConverter;

	internal TextDecorationStyleProperty()
		: base(PropertyNames.TextDecorationStyle)
	{
	}
}

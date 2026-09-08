namespace ExCSS;

internal sealed class TextDecorationLineProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.TextDecorationLinesConverter.OrDefault();

	internal override IValueConverter Converter => ListConverter;

	internal TextDecorationLineProperty()
		: base(PropertyNames.TextDecorationLine)
	{
	}
}

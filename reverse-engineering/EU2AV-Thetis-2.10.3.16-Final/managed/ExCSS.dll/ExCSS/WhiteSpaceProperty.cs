namespace ExCSS;

internal sealed class WhiteSpaceProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.WhitespaceConverter.OrDefault(Whitespace.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal WhiteSpaceProperty()
		: base(PropertyNames.WhiteSpace, PropertyFlags.Inherited)
	{
	}
}

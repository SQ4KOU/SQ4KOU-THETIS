namespace ExCSS;

internal sealed class QuotesProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.EvenStringsConverter.OrNone().OrDefault(new string[2] { "«", "»" });

	internal override IValueConverter Converter => StyleConverter;

	internal QuotesProperty()
		: base(PropertyNames.Quotes, PropertyFlags.Inherited)
	{
	}
}

namespace ExCSS;

internal sealed class WordBreakProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.WordBreakConverter;

	internal override IValueConverter Converter => StyleConverter;

	public WordBreakProperty()
		: base(PropertyNames.WordBreak)
	{
	}
}

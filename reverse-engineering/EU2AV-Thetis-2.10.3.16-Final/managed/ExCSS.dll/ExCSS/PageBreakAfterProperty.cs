namespace ExCSS;

internal sealed class PageBreakAfterProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.PageBreakModeConverter.OrDefault(BreakMode.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal PageBreakAfterProperty()
		: base(PropertyNames.PageBreakAfter)
	{
	}
}

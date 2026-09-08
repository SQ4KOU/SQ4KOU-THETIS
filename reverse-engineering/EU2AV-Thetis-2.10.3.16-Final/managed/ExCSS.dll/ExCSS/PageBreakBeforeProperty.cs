namespace ExCSS;

internal sealed class PageBreakBeforeProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.PageBreakModeConverter.OrDefault(BreakMode.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal PageBreakBeforeProperty()
		: base(PropertyNames.PageBreakBefore)
	{
	}
}

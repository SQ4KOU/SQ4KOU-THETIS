namespace ExCSS;

internal sealed class BreakAfterProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BreakModeConverter.OrDefault(BreakMode.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal BreakAfterProperty()
		: base(PropertyNames.BreakAfter)
	{
	}
}

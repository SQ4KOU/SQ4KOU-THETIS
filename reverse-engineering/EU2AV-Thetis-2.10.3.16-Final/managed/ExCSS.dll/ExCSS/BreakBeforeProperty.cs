namespace ExCSS;

internal sealed class BreakBeforeProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BreakModeConverter.OrDefault(BreakMode.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal BreakBeforeProperty()
		: base(PropertyNames.BreakBefore)
	{
	}
}

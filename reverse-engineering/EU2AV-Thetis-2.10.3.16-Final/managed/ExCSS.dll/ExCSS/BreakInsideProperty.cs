namespace ExCSS;

internal sealed class BreakInsideProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BreakInsideModeConverter.OrDefault(BreakMode.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal BreakInsideProperty()
		: base(PropertyNames.BreakInside)
	{
	}
}

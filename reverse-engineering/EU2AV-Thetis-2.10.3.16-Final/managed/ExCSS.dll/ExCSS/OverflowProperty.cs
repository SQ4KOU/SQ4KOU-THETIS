namespace ExCSS;

internal sealed class OverflowProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OverflowModeConverter.OrDefault(Overflow.Visible);

	internal override IValueConverter Converter => StyleConverter;

	internal OverflowProperty()
		: base(PropertyNames.Overflow)
	{
	}
}

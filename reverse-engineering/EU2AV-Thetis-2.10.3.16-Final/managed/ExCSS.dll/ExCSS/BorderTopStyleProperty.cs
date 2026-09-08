namespace ExCSS;

internal sealed class BorderTopStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderTopStyleProperty()
		: base(PropertyNames.BorderTopStyle)
	{
	}
}

namespace ExCSS;

internal sealed class BorderBottomStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderBottomStyleProperty()
		: base(PropertyNames.BorderBottomStyle)
	{
	}
}

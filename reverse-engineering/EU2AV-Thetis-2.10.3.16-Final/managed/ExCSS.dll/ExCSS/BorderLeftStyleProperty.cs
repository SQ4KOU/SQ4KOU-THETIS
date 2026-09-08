namespace ExCSS;

internal sealed class BorderLeftStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderLeftStyleProperty()
		: base(PropertyNames.BorderLeftStyle)
	{
	}
}

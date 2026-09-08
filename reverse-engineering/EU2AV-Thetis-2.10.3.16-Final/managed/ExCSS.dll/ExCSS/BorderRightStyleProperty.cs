namespace ExCSS;

internal sealed class BorderRightStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderRightStyleProperty()
		: base(PropertyNames.BorderRightStyle)
	{
	}
}

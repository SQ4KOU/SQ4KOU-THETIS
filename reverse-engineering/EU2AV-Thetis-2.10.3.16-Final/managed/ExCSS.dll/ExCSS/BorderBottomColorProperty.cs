namespace ExCSS;

internal sealed class BorderBottomColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.OrDefault(Color.Transparent);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderBottomColorProperty()
		: base(PropertyNames.BorderBottomColor)
	{
	}
}

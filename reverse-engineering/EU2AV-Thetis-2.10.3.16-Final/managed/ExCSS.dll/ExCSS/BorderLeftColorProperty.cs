namespace ExCSS;

internal sealed class BorderLeftColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.OrDefault(Color.Transparent);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderLeftColorProperty()
		: base(PropertyNames.BorderLeftColor)
	{
	}
}

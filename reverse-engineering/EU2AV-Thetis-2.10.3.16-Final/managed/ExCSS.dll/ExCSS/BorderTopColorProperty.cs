namespace ExCSS;

internal sealed class BorderTopColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.OrDefault(Color.Transparent);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderTopColorProperty()
		: base(PropertyNames.BorderTopColor)
	{
	}
}

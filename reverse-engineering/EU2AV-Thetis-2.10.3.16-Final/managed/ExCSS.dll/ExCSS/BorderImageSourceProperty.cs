namespace ExCSS;

internal sealed class BorderImageSourceProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalImageSourceConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderImageSourceProperty()
		: base(PropertyNames.BorderImageSource)
	{
	}
}

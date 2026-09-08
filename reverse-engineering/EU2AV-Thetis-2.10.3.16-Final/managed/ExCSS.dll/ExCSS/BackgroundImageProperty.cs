namespace ExCSS;

internal sealed class BackgroundImageProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.MultipleImageSourceConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BackgroundImageProperty()
		: base(PropertyNames.BackgroundImage)
	{
	}
}

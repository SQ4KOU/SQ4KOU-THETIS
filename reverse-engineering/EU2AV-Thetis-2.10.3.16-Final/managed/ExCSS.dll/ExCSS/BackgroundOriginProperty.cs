namespace ExCSS;

internal sealed class BackgroundOriginProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.BoxModelConverter.FromList().OrDefault(BoxModel.PaddingBox);

	internal override IValueConverter Converter => ListConverter;

	internal BackgroundOriginProperty()
		: base(PropertyNames.BackgroundOrigin)
	{
	}
}

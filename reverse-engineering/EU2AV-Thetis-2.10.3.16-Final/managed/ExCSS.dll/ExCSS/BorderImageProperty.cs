namespace ExCSS;

internal sealed class BorderImageProperty : ShorthandProperty
{
	private static readonly IValueConverter ImageConverter = Converters.WithAny(Converters.OptionalImageSourceConverter.Option().For(PropertyNames.BorderImageSource), Converters.WithOrder(BorderImageSliceProperty.TheConverter.Option().For(PropertyNames.BorderImageSlice), BorderImageWidthProperty.TheConverter.StartsWithDelimiter().Option().For(PropertyNames.BorderImageWidth), BorderImageOutsetProperty.TheConverter.StartsWithDelimiter().Option().For(PropertyNames.BorderImageOutset)), BorderImageRepeatProperty.TheConverter.Option().For(PropertyNames.BorderImageRepeat)).OrDefault();

	internal override IValueConverter Converter => ImageConverter;

	internal BorderImageProperty()
		: base(PropertyNames.BorderImage)
	{
	}
}

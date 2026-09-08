namespace ExCSS;

internal sealed class BackgroundProperty : ShorthandProperty
{
	private static readonly IValueConverter NormalLayerConverter = Converters.WithAny(Converters.OptionalImageSourceConverter.Option().For(PropertyNames.BackgroundImage), Converters.WithOrder(Converters.PointConverter.Option().For(PropertyNames.BackgroundPosition), Converters.BackgroundSizeConverter.StartsWithDelimiter().Option().For(PropertyNames.BackgroundSize)), Converters.BackgroundRepeatsConverter.Option().For(PropertyNames.BackgroundRepeat), Converters.BackgroundAttachmentConverter.Option().For(PropertyNames.BackgroundAttachment), Converters.BoxModelConverter.Option().For(PropertyNames.BackgroundOrigin), Converters.BoxModelConverter.Option().For(PropertyNames.BackgroundClip));

	private static readonly IValueConverter FinalLayerConverter = Converters.WithAny(Converters.OptionalImageSourceConverter.Option().For(PropertyNames.BackgroundImage), Converters.WithOrder(Converters.PointConverter.Option().For(PropertyNames.BackgroundPosition), Converters.BackgroundSizeConverter.StartsWithDelimiter().Option().For(PropertyNames.BackgroundSize)), Converters.BackgroundRepeatsConverter.Option().For(PropertyNames.BackgroundRepeat), Converters.BackgroundAttachmentConverter.Option().For(PropertyNames.BackgroundAttachment), Converters.BoxModelConverter.Option().For(PropertyNames.BackgroundOrigin), Converters.BoxModelConverter.Option().For(PropertyNames.BackgroundClip), Converters.CurrentColorConverter.Option().For(PropertyNames.BackgroundColor));

	private static readonly IValueConverter StyleConverter = NormalLayerConverter.RequiresEnd(FinalLayerConverter).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BackgroundProperty()
		: base(PropertyNames.Background, PropertyFlags.Animatable)
	{
	}
}

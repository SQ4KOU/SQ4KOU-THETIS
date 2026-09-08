namespace ExCSS;

internal sealed class FontProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithOrder(Converters.WithAny(Converters.FontStyleConverter.Option().For(PropertyNames.FontStyle), Converters.FontVariantConverter.Option().For(PropertyNames.FontVariant), Converters.FontWeightConverter.Or(Converters.WeightIntegerConverter).Option().For(PropertyNames.FontWeight), Converters.FontStretchConverter.Option().For(PropertyNames.FontStretch)), Converters.WithOrder(Converters.FontSizeConverter.Required().For(PropertyNames.FontSize), Converters.LineHeightConverter.StartsWithDelimiter().Option().For(PropertyNames.LineHeight), Converters.FontFamiliesConverter.Required().For(PropertyNames.FontFamily))).Or(Converters.SystemFontConverter).OrGlobalValue();

	internal override IValueConverter Converter => StyleConverter;

	internal FontProperty()
		: base(PropertyNames.Font, PropertyFlags.Inherited | PropertyFlags.Animatable)
	{
	}
}

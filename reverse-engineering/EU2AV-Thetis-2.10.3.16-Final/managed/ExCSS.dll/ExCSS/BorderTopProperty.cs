namespace ExCSS;

internal sealed class BorderTopProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.LineWidthConverter.Option().For(PropertyNames.BorderTopWidth), Converters.LineStyleConverter.Option().For(PropertyNames.BorderTopStyle), Converters.CurrentColorConverter.Option().For(PropertyNames.BorderTopColor)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderTopProperty()
		: base(PropertyNames.BorderTop, PropertyFlags.Animatable)
	{
	}
}

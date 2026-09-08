namespace ExCSS;

internal sealed class BorderBottomProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.LineWidthConverter.Option().For(PropertyNames.BorderBottomWidth), Converters.LineStyleConverter.Option().For(PropertyNames.BorderBottomStyle), Converters.CurrentColorConverter.Option().For(PropertyNames.BorderBottomColor)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderBottomProperty()
		: base(PropertyNames.BorderBottom, PropertyFlags.Animatable)
	{
	}
}

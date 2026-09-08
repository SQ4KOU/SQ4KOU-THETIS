namespace ExCSS;

internal sealed class BorderProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.LineWidthConverter.Option().For(PropertyNames.BorderTopWidth, PropertyNames.BorderRightWidth, PropertyNames.BorderBottomWidth, PropertyNames.BorderLeftWidth), Converters.LineStyleConverter.Option().For(PropertyNames.BorderTopStyle, PropertyNames.BorderRightStyle, PropertyNames.BorderBottomStyle, PropertyNames.BorderLeftStyle), Converters.CurrentColorConverter.Option().For(PropertyNames.BorderTopColor, PropertyNames.BorderRightColor, PropertyNames.BorderBottomColor, PropertyNames.BorderLeftColor)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderProperty()
		: base(PropertyNames.Border, PropertyFlags.Animatable)
	{
	}
}

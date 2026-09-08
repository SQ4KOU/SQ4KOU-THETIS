namespace ExCSS;

internal sealed class BorderWidthProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.Periodic(PropertyNames.BorderTopWidth, PropertyNames.BorderRightWidth, PropertyNames.BorderBottomWidth, PropertyNames.BorderLeftWidth).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderWidthProperty()
		: base(PropertyNames.BorderWidth, PropertyFlags.Animatable)
	{
	}
}

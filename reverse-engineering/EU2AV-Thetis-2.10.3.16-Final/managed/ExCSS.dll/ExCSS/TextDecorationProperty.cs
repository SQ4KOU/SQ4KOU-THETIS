namespace ExCSS;

internal sealed class TextDecorationProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.ColorConverter.Option().For(PropertyNames.TextDecorationColor), Converters.TextDecorationStyleConverter.Option().For(PropertyNames.TextDecorationStyle), Converters.TextDecorationLinesConverter.Option().For(PropertyNames.TextDecorationLine)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal TextDecorationProperty()
		: base(PropertyNames.TextDecoration, PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class TextDecorationColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ColorConverter.OrDefault(Color.Black);

	internal override IValueConverter Converter => StyleConverter;

	internal TextDecorationColorProperty()
		: base(PropertyNames.TextDecorationColor, PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class ColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ColorConverter.OrDefault(Color.Black);

	internal override IValueConverter Converter => StyleConverter;

	internal ColorProperty()
		: base(PropertyNames.Color, PropertyFlags.Inherited | PropertyFlags.Hashless | PropertyFlags.Animatable)
	{
	}
}

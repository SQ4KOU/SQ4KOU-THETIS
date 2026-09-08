namespace ExCSS;

internal sealed class OutlineColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.InvertedColorConverter.OrDefault(Color.Transparent);

	internal override IValueConverter Converter => StyleConverter;

	internal OutlineColorProperty()
		: base(PropertyNames.OutlineColor, PropertyFlags.Animatable)
	{
	}
}

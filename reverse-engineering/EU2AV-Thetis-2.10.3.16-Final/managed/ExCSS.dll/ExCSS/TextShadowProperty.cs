namespace ExCSS;

internal sealed class TextShadowProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.MultipleShadowConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal TextShadowProperty()
		: base(PropertyNames.TextShadow, PropertyFlags.Inherited | PropertyFlags.Animatable)
	{
	}
}

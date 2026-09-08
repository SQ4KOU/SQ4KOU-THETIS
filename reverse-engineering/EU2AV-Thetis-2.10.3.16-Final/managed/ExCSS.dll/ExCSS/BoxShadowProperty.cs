namespace ExCSS;

internal sealed class BoxShadowProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.MultipleShadowConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BoxShadowProperty()
		: base(PropertyNames.BoxShadow, PropertyFlags.Animatable)
	{
	}
}

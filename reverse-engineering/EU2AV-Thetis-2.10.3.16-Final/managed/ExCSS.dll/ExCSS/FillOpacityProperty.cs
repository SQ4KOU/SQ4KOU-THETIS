namespace ExCSS;

internal sealed class FillOpacityProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.NumberConverter.OrDefault(1f);

	internal override IValueConverter Converter => StyleConverter;

	internal FillOpacityProperty()
		: base(PropertyNames.FillOpacity, PropertyFlags.Animatable)
	{
	}
}

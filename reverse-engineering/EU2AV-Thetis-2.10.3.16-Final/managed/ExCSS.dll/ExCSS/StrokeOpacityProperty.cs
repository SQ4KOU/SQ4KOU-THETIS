namespace ExCSS;

internal sealed class StrokeOpacityProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.NumberConverter.OrDefault(1f);

	internal override IValueConverter Converter => StyleConverter;

	internal StrokeOpacityProperty()
		: base(PropertyNames.StrokeOpacity, PropertyFlags.Animatable)
	{
	}
}

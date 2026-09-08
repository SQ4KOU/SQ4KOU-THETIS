namespace ExCSS;

internal sealed class ZIndexProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalIntegerConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ZIndexProperty()
		: base(PropertyNames.ZIndex, PropertyFlags.Animatable)
	{
	}
}

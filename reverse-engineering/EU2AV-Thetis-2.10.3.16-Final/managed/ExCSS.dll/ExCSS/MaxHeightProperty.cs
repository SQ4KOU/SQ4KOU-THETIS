namespace ExCSS;

internal sealed class MaxHeightProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalLengthOrPercentConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal MaxHeightProperty()
		: base(PropertyNames.MaxHeight, PropertyFlags.Animatable)
	{
	}
}

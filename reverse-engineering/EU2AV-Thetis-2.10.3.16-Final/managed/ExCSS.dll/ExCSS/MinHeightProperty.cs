namespace ExCSS;

internal sealed class MinHeightProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal MinHeightProperty()
		: base(PropertyNames.MinHeight, PropertyFlags.Animatable)
	{
	}
}

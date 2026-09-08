namespace ExCSS;

internal sealed class MinWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal MinWidthProperty()
		: base(PropertyNames.MinWidth, PropertyFlags.Animatable)
	{
	}
}

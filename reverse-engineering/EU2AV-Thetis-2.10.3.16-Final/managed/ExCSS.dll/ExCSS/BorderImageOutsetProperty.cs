namespace ExCSS;

internal sealed class BorderImageOutsetProperty : Property
{
	internal static readonly IValueConverter TheConverter = Converters.LengthOrPercentConverter.Periodic();

	private static readonly IValueConverter StyleConverter = TheConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderImageOutsetProperty()
		: base(PropertyNames.BorderImageOutset)
	{
	}
}

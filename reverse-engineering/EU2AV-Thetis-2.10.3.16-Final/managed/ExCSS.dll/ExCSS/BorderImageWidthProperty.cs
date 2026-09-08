namespace ExCSS;

internal sealed class BorderImageWidthProperty : Property
{
	internal static readonly IValueConverter TheConverter = Converters.ImageBorderWidthConverter.Periodic();

	private static readonly IValueConverter StyleConverter = TheConverter.OrDefault(Length.Full);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderImageWidthProperty()
		: base(PropertyNames.BorderImageWidth)
	{
	}
}

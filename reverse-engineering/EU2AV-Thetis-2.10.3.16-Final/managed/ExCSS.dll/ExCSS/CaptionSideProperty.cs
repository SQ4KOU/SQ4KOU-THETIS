namespace ExCSS;

internal sealed class CaptionSideProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.CaptionSideConverter.OrDefault(value: true);

	internal override IValueConverter Converter => StyleConverter;

	internal CaptionSideProperty()
		: base(PropertyNames.CaptionSide)
	{
	}
}

namespace ExCSS;

internal sealed class FontSizeAdjustProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalNumberConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal FontSizeAdjustProperty()
		: base(PropertyNames.FontSizeAdjust, PropertyFlags.Inherited | PropertyFlags.Animatable)
	{
	}
}

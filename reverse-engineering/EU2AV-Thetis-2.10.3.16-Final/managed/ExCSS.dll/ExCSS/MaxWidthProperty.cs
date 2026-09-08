namespace ExCSS;

internal sealed class MaxWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalLengthOrPercentConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal MaxWidthProperty()
		: base(PropertyNames.MaxWidth, PropertyFlags.Animatable)
	{
	}
}

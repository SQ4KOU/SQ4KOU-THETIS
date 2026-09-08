namespace ExCSS;

internal sealed class WordSpacingProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalLengthConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal WordSpacingProperty()
		: base(PropertyNames.WordSpacing, PropertyFlags.Inherited | PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

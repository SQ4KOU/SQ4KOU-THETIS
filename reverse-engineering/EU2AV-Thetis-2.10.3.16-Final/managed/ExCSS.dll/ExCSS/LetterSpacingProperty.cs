namespace ExCSS;

internal sealed class LetterSpacingProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalLengthConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal LetterSpacingProperty()
		: base(PropertyNames.LetterSpacing, PropertyFlags.Inherited | PropertyFlags.Unitless)
	{
	}
}

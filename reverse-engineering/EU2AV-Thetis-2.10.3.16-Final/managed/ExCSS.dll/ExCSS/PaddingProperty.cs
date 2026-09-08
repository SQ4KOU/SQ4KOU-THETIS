namespace ExCSS;

internal sealed class PaddingProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.Periodic(PropertyNames.PaddingTop, PropertyNames.PaddingRight, PropertyNames.PaddingBottom, PropertyNames.PaddingLeft).OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal PaddingProperty()
		: base(PropertyNames.Padding)
	{
	}
}

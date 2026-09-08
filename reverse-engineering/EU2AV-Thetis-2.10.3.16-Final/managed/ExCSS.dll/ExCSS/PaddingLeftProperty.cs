namespace ExCSS;

internal sealed class PaddingLeftProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal PaddingLeftProperty()
		: base(PropertyNames.PaddingLeft, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

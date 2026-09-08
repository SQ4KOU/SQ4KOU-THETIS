namespace ExCSS;

internal sealed class PaddingBottomProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal PaddingBottomProperty()
		: base(PropertyNames.PaddingBottom, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

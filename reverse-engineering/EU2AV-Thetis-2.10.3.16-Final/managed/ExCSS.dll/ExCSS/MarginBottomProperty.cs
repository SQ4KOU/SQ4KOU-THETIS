namespace ExCSS;

internal sealed class MarginBottomProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal MarginBottomProperty()
		: base(PropertyNames.MarginBottom, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

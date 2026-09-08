namespace ExCSS;

internal sealed class BottomProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal BottomProperty()
		: base(PropertyNames.Bottom, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

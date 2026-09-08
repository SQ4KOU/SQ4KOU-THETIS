namespace ExCSS;

internal sealed class HeightProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal HeightProperty()
		: base(PropertyNames.Height, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

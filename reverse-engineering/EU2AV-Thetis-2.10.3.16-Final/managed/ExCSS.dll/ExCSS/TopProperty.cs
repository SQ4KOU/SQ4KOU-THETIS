namespace ExCSS;

internal sealed class TopProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal TopProperty()
		: base(PropertyNames.Top, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

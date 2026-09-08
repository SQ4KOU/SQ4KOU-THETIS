namespace ExCSS;

internal sealed class LeftProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal LeftProperty()
		: base(PropertyNames.Left, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

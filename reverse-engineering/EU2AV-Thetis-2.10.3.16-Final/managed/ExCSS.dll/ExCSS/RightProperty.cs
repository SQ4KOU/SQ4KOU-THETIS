namespace ExCSS;

internal sealed class RightProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal RightProperty()
		: base(PropertyNames.Right, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

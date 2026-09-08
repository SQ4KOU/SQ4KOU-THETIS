namespace ExCSS;

internal sealed class WidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal WidthProperty()
		: base(PropertyNames.Width, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

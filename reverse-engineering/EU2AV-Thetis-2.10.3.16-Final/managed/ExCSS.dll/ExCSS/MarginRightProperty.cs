namespace ExCSS;

internal sealed class MarginRightProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal MarginRightProperty()
		: base(PropertyNames.MarginRight, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

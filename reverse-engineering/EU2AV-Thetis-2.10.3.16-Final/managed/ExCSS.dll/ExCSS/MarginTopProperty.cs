namespace ExCSS;

internal sealed class MarginTopProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal MarginTopProperty()
		: base(PropertyNames.MarginTop, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class MarginLeftProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal MarginLeftProperty()
		: base(PropertyNames.MarginLeft, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

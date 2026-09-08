namespace ExCSS;

internal sealed class FontWeightProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FontWeightConverter.Or(Converters.WeightIntegerConverter).OrDefault(FontWeight.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal FontWeightProperty()
		: base(PropertyNames.FontWeight, PropertyFlags.Inherited | PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class VerticalAlignProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.Or(Converters.VerticalAlignmentConverter).OrDefault(VerticalAlignment.Baseline);

	internal override IValueConverter Converter => StyleConverter;

	internal VerticalAlignProperty()
		: base(PropertyNames.VerticalAlign, PropertyFlags.Animatable)
	{
	}
}

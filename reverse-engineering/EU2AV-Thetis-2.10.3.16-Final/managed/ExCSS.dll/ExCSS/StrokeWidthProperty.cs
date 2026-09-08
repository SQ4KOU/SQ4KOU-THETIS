namespace ExCSS;

internal sealed class StrokeWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal StrokeWidthProperty()
		: base(PropertyNames.StrokeWidth, PropertyFlags.Animatable)
	{
	}
}

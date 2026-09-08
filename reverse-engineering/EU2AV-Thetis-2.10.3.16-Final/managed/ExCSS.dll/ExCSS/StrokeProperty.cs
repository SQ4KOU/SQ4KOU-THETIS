namespace ExCSS;

internal sealed class StrokeProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.PaintConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal StrokeProperty()
		: base(PropertyNames.Stroke, PropertyFlags.Animatable)
	{
	}
}

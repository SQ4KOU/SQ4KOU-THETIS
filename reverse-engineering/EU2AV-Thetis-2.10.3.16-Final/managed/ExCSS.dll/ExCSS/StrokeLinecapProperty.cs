namespace ExCSS;

internal sealed class StrokeLinecapProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.StrokeLinecapConverter.OrDefault(StrokeLinecap.Butt);

	internal override IValueConverter Converter => StyleConverter;

	public StrokeLinecapProperty()
		: base(PropertyNames.StrokeLinecap, PropertyFlags.Animatable)
	{
	}
}

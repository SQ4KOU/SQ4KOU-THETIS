namespace ExCSS;

internal sealed class StrokeLinejoinProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.StrokeLinejoinConverter;

	internal override IValueConverter Converter => StyleConverter;

	public StrokeLinejoinProperty()
		: base(PropertyNames.StrokeLinejoin, PropertyFlags.Animatable)
	{
	}
}

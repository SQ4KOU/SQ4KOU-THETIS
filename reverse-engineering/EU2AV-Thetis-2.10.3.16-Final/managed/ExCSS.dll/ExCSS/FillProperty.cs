namespace ExCSS;

internal sealed class FillProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.PaintConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FillProperty()
		: base(PropertyNames.Fill, PropertyFlags.Animatable)
	{
	}
}

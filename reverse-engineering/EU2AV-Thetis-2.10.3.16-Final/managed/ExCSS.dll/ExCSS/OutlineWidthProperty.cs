namespace ExCSS;

internal sealed class OutlineWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

	internal override IValueConverter Converter => StyleConverter;

	internal OutlineWidthProperty()
		: base(PropertyNames.OutlineWidth, PropertyFlags.Animatable)
	{
	}
}

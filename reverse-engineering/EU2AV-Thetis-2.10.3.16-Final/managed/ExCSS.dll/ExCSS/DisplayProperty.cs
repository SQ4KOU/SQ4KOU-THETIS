namespace ExCSS;

internal sealed class DisplayProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.DisplayModeConverter.OrDefault(DisplayMode.Inline);

	internal override IValueConverter Converter => StyleConverter;

	internal DisplayProperty()
		: base(PropertyNames.Display)
	{
	}
}

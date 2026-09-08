namespace ExCSS;

internal sealed class ClearProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ClearModeConverter.OrDefault(ClearMode.None);

	internal override IValueConverter Converter => StyleConverter;

	internal ClearProperty()
		: base(PropertyNames.Clear)
	{
	}
}

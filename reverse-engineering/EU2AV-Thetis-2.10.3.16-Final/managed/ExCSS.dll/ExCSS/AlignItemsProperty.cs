namespace ExCSS;

internal sealed class AlignItemsProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AlignItemsConverter.OrDefault(Keywords.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal AlignItemsProperty()
		: base(PropertyNames.AlignItems)
	{
	}
}

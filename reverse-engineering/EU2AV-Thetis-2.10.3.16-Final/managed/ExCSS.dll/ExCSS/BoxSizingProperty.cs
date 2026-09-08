namespace ExCSS;

internal class BoxSizingProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BoxSizingConverter.OrDefault(Keywords.ContentBox);

	internal override IValueConverter Converter => StyleConverter;

	public BoxSizingProperty()
		: base(PropertyNames.BoxSizing)
	{
	}
}

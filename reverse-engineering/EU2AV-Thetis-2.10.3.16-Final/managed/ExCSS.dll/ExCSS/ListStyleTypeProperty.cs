namespace ExCSS;

internal sealed class ListStyleTypeProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ListStyleConverter.OrDefault(ListStyle.Disc);

	internal override IValueConverter Converter => StyleConverter;

	internal ListStyleTypeProperty()
		: base(PropertyNames.ListStyleType, PropertyFlags.Inherited)
	{
	}
}

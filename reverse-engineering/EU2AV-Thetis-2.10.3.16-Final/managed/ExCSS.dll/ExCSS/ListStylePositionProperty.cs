namespace ExCSS;

internal sealed class ListStylePositionProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ListPositionConverter.OrDefault(ListPosition.Outside);

	internal override IValueConverter Converter => StyleConverter;

	internal ListStylePositionProperty()
		: base(PropertyNames.ListStylePosition, PropertyFlags.Inherited)
	{
	}
}

namespace ExCSS;

internal sealed class ContainerTypeProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ContainerTypeConverter.OrDefault(Keywords.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal ContainerTypeProperty()
		: base(PropertyNames.ContainerType)
	{
	}
}

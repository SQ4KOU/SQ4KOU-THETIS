namespace ExCSS;

internal sealed class ListStyleImageProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalImageSourceConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ListStyleImageProperty()
		: base(PropertyNames.ListStyleImage, PropertyFlags.Inherited)
	{
	}
}

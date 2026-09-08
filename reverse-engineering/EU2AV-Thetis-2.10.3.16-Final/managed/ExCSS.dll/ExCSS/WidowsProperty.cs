namespace ExCSS;

internal sealed class WidowsProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.IntegerConverter.OrDefault(2);

	internal override IValueConverter Converter => StyleConverter;

	internal WidowsProperty()
		: base(PropertyNames.Widows, PropertyFlags.Inherited)
	{
	}
}

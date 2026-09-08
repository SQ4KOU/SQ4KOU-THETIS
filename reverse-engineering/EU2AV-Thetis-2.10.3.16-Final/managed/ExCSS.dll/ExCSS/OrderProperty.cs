namespace ExCSS;

internal sealed class OrderProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.IntegerConverter.OrGlobalValue().OrDefault(0);

	internal override IValueConverter Converter => StyleConverter;

	internal OrderProperty()
		: base(PropertyNames.Order)
	{
	}
}

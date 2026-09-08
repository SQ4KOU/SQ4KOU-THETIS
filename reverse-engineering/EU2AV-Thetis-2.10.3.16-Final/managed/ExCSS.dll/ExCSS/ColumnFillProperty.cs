namespace ExCSS;

internal sealed class ColumnFillProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ColumnFillConverter.OrDefault(value: true);

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnFillProperty()
		: base(PropertyNames.ColumnFill)
	{
	}
}

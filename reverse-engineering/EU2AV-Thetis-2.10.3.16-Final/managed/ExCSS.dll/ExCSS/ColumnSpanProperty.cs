namespace ExCSS;

internal sealed class ColumnSpanProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ColumnSpanConverter.OrDefault(value: false);

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnSpanProperty()
		: base(PropertyNames.ColumnSpan)
	{
	}
}

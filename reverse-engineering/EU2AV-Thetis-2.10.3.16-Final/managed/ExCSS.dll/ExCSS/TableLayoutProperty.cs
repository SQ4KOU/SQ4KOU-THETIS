namespace ExCSS;

internal sealed class TableLayoutProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.TableLayoutConverter.OrDefault(value: false);

	internal override IValueConverter Converter => StyleConverter;

	internal TableLayoutProperty()
		: base(PropertyNames.TableLayout)
	{
	}
}

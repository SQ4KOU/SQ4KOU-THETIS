namespace ExCSS;

internal sealed class EmptyCellsProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.EmptyCellsConverter.OrDefault(value: true);

	internal override IValueConverter Converter => StyleConverter;

	internal EmptyCellsProperty()
		: base(PropertyNames.EmptyCells, PropertyFlags.Inherited)
	{
	}
}

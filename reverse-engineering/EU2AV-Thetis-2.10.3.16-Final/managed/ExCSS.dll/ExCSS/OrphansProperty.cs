namespace ExCSS;

internal sealed class OrphansProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.NaturalIntegerConverter.OrDefault(2);

	internal override IValueConverter Converter => StyleConverter;

	internal OrphansProperty()
		: base(PropertyNames.Orphans, PropertyFlags.Inherited)
	{
	}
}

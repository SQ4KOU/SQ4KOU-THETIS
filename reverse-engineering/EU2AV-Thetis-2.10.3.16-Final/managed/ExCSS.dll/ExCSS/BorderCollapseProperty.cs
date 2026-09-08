namespace ExCSS;

internal sealed class BorderCollapseProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BorderCollapseConverter.OrDefault(value: true);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderCollapseProperty()
		: base(PropertyNames.BorderCollapse, PropertyFlags.Inherited)
	{
	}
}

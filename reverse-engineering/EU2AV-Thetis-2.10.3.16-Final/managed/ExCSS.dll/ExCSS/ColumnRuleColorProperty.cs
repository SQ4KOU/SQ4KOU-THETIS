namespace ExCSS;

internal sealed class ColumnRuleColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ColorConverter.OrDefault(Color.Transparent);

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnRuleColorProperty()
		: base(PropertyNames.ColumnRuleColor, PropertyFlags.Animatable)
	{
	}
}

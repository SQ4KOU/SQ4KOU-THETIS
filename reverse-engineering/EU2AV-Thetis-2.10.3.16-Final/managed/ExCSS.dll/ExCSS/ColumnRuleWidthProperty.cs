namespace ExCSS;

internal sealed class ColumnRuleWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnRuleWidthProperty()
		: base(PropertyNames.ColumnRuleWidth, PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class ColumnRuleStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnRuleStyleProperty()
		: base(PropertyNames.ColumnRuleStyle)
	{
	}
}

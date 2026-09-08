namespace ExCSS;

internal sealed class ColumnRuleProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.ColorConverter.Option().For(PropertyNames.ColumnRuleColor), Converters.LineWidthConverter.Option().For(PropertyNames.ColumnRuleWidth), Converters.LineStyleConverter.Option().For(PropertyNames.ColumnRuleStyle)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnRuleProperty()
		: base(PropertyNames.ColumnRule, PropertyFlags.Animatable)
	{
	}
}

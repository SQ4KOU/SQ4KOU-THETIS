namespace ExCSS;

internal class GapProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrGlobalValue().Periodic(PropertyNames.RowGap, PropertyNames.ColumnGap);

	internal override IValueConverter Converter => StyleConverter;

	internal GapProperty()
		: base(PropertyNames.Gap, PropertyFlags.Animatable)
	{
	}
}

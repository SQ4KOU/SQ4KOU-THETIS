namespace ExCSS;

internal sealed class ColumnsProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.AutoLengthConverter.Option().For(PropertyNames.ColumnWidth), Converters.OptionalIntegerConverter.Option().For(PropertyNames.ColumnCount)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnsProperty()
		: base(PropertyNames.Columns, PropertyFlags.Animatable)
	{
	}
}

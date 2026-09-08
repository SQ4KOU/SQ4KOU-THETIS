namespace ExCSS;

internal sealed class ColumnWidthProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AutoLengthConverter.OrDefault(Keywords.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnWidthProperty()
		: base(PropertyNames.ColumnWidth, PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class ColumnCountProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalIntegerConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ColumnCountProperty()
		: base(PropertyNames.ColumnCount, PropertyFlags.Animatable)
	{
	}
}

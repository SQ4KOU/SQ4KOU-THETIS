namespace ExCSS;

internal sealed class FlexProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.FlexConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FlexProperty()
		: base(PropertyNames.Flex)
	{
	}
}

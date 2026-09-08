namespace ExCSS;

internal sealed class UnicodeBidirectionalProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.UnicodeModeConverter.OrDefault(UnicodeMode.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal UnicodeBidirectionalProperty()
		: base(PropertyNames.UnicodeBidirectional)
	{
	}
}

namespace ExCSS;

internal sealed class BoxDecorationBreak : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BoxDecorationConverter.OrDefault(value: false);

	internal override IValueConverter Converter => StyleConverter;

	internal BoxDecorationBreak()
		: base(PropertyNames.BoxDecorationBreak)
	{
	}
}

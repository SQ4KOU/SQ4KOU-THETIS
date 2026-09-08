namespace ExCSS;

internal sealed class TextAlignProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.HorizontalAlignmentConverter.OrDefault(HorizontalAlignment.Left);

	internal override IValueConverter Converter => StyleConverter;

	internal TextAlignProperty()
		: base(PropertyNames.TextAlign, PropertyFlags.Inherited)
	{
	}
}

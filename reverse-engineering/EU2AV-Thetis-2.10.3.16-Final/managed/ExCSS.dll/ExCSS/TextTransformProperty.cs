namespace ExCSS;

internal sealed class TextTransformProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.TextTransformConverter.OrDefault(TextTransform.None);

	internal override IValueConverter Converter => StyleConverter;

	internal TextTransformProperty()
		: base(PropertyNames.TextTransform, PropertyFlags.Inherited)
	{
	}
}

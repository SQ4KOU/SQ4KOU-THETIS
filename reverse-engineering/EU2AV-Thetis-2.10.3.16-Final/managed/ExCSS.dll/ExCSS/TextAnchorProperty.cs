namespace ExCSS;

internal sealed class TextAnchorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.TextAnchorConverter;

	internal override IValueConverter Converter => StyleConverter;

	public TextAnchorProperty()
		: base(PropertyNames.TextAnchor)
	{
	}
}

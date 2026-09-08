namespace ExCSS;

internal sealed class TextJustifyProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.TextJustifyConverter;

	internal override IValueConverter Converter => StyleConverter;

	public TextJustifyProperty()
		: base(PropertyNames.TextJustify)
	{
	}
}

namespace ExCSS;

internal sealed class TextAlignLastProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.TextAlignLastConverter;

	internal override IValueConverter Converter => StyleConverter;

	public TextAlignLastProperty()
		: base(PropertyNames.TextAlignLast)
	{
	}
}

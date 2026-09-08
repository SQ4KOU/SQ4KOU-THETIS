namespace ExCSS;

internal sealed class AlignContentProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AlignContentConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal AlignContentProperty()
		: base(PropertyNames.AlignContent)
	{
	}
}

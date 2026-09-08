namespace ExCSS;

internal sealed class JustifyContentProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.JustifyContentConverter.OrDefault(Keywords.Normal);

	internal override IValueConverter Converter => StyleConverter;

	internal JustifyContentProperty()
		: base(PropertyNames.JustifyContent)
	{
	}
}

namespace ExCSS;

internal sealed class FontFamilyProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FontFamiliesConverter.OrDefault("Times New Roman");

	internal override IValueConverter Converter => StyleConverter;

	internal FontFamilyProperty()
		: base(PropertyNames.FontFamily, PropertyFlags.Inherited)
	{
	}
}

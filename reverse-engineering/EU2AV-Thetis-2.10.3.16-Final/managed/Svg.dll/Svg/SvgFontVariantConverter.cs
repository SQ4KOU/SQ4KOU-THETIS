namespace Svg;

public sealed class SvgFontVariantConverter : EnumBaseConverter<SvgFontVariant>
{
	public SvgFontVariantConverter()
		: base(CaseHandling.KebabCase)
	{
	}
}

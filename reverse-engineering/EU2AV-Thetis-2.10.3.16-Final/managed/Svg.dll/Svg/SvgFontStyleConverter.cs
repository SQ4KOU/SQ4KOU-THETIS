namespace Svg;

public sealed class SvgFontStyleConverter : EnumBaseConverter<SvgFontStyle>
{
	public SvgFontStyleConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

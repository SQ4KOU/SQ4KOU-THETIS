namespace Svg;

public sealed class SvgFontStretchConverter : EnumBaseConverter<SvgFontStretch>
{
	public SvgFontStretchConverter()
		: base(CaseHandling.KebabCase)
	{
	}
}

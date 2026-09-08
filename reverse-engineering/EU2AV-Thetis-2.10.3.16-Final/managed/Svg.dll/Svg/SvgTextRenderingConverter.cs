namespace Svg;

public sealed class SvgTextRenderingConverter : EnumBaseConverter<SvgTextRendering>
{
	public SvgTextRenderingConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

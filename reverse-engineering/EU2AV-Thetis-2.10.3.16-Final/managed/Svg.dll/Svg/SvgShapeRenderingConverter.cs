namespace Svg;

public sealed class SvgShapeRenderingConverter : EnumBaseConverter<SvgShapeRendering>
{
	public SvgShapeRenderingConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

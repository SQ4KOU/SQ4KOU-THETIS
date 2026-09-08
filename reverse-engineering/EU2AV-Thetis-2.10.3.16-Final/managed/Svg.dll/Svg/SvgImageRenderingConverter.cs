namespace Svg;

public sealed class SvgImageRenderingConverter : EnumBaseConverter<SvgImageRendering>
{
	public SvgImageRenderingConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

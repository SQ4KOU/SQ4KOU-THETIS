namespace Svg;

public sealed class SvgTextAnchorConverter : EnumBaseConverter<SvgTextAnchor>
{
	public SvgTextAnchorConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

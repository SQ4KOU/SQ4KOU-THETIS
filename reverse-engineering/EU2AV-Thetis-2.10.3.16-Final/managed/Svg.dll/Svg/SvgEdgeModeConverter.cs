using Svg.FilterEffects;

namespace Svg;

public sealed class SvgEdgeModeConverter : EnumBaseConverter<SvgEdgeMode>
{
	public SvgEdgeModeConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

using Svg.FilterEffects;

namespace Svg;

public sealed class SvgMorphologyOperatorConverter : EnumBaseConverter<SvgMorphologyOperator>
{
	public SvgMorphologyOperatorConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

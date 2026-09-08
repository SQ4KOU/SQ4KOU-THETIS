using Svg.FilterEffects;

namespace Svg;

public sealed class SvgCompositeOperatorConverter : EnumBaseConverter<SvgCompositeOperator>
{
	public SvgCompositeOperatorConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

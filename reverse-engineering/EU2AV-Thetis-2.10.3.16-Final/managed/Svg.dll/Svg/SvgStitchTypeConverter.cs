using Svg.FilterEffects;

namespace Svg;

public sealed class SvgStitchTypeConverter : EnumBaseConverter<SvgStitchType>
{
	public SvgStitchTypeConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

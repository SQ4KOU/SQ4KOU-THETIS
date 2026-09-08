using Svg.FilterEffects;

namespace Svg;

public sealed class SvgBlendModeConverter : EnumBaseConverter<SvgBlendMode>
{
	public SvgBlendModeConverter()
		: base(CaseHandling.KebabCase)
	{
	}
}

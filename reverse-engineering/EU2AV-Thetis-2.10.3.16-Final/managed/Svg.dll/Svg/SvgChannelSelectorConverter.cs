using Svg.FilterEffects;

namespace Svg;

public sealed class SvgChannelSelectorConverter : EnumBaseConverter<SvgChannelSelector>
{
	public SvgChannelSelectorConverter()
		: base(CaseHandling.PascalCase)
	{
	}
}

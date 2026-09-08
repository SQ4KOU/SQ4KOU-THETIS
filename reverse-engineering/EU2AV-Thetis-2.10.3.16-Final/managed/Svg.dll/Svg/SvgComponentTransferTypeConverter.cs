using Svg.FilterEffects;

namespace Svg;

public sealed class SvgComponentTransferTypeConverter : EnumBaseConverter<SvgComponentTransferType>
{
	public SvgComponentTransferTypeConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

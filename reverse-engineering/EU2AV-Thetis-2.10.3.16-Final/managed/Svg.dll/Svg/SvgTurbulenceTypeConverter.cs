using Svg.FilterEffects;

namespace Svg;

public sealed class SvgTurbulenceTypeConverter : EnumBaseConverter<SvgTurbulenceType>
{
	public SvgTurbulenceTypeConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

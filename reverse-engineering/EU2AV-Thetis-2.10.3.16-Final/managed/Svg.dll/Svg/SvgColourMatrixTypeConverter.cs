using Svg.FilterEffects;

namespace Svg;

public sealed class SvgColourMatrixTypeConverter : EnumBaseConverter<SvgColourMatrixType>
{
	public SvgColourMatrixTypeConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

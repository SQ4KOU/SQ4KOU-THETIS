namespace Svg;

public sealed class SvgStrokeLineCapConverter : EnumBaseConverter<SvgStrokeLineCap>
{
	public SvgStrokeLineCapConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

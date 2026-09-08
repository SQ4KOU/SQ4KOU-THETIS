namespace Svg;

public sealed class SvgStrokeLineJoinConverter : EnumBaseConverter<SvgStrokeLineJoin>
{
	public SvgStrokeLineJoinConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

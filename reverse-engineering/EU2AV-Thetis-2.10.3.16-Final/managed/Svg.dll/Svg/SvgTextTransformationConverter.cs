namespace Svg;

public sealed class SvgTextTransformationConverter : EnumBaseConverter<SvgTextTransformation>
{
	public SvgTextTransformationConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

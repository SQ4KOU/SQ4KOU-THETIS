namespace Svg;

public sealed class SvgGradientSpreadMethodConverter : EnumBaseConverter<SvgGradientSpreadMethod>
{
	public SvgGradientSpreadMethodConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

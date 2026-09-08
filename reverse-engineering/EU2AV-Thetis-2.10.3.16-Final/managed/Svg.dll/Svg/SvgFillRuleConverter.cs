namespace Svg;

public sealed class SvgFillRuleConverter : EnumBaseConverter<SvgFillRule>
{
	public SvgFillRuleConverter()
		: base(CaseHandling.LowerCase)
	{
	}
}

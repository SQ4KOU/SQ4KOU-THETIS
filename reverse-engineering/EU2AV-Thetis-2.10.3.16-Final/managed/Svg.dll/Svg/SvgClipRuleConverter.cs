namespace Svg;

public sealed class SvgClipRuleConverter : EnumBaseConverter<SvgClipRule>
{
	public SvgClipRuleConverter()
		: base(CaseHandling.LowerCase)
	{
	}
}

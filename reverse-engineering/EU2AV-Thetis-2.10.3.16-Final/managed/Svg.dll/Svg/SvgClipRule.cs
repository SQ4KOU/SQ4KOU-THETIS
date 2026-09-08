using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgClipRuleConverter))]
public enum SvgClipRule
{
	NonZero,
	EvenOdd,
	Inherit
}

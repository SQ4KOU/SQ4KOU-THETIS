using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgTextPathSpacingConverter))]
public enum SvgTextPathSpacing
{
	Exact,
	Auto
}

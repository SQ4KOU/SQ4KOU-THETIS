using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgTextAnchorConverter))]
public enum SvgTextAnchor
{
	Inherit,
	Start,
	Middle,
	End
}

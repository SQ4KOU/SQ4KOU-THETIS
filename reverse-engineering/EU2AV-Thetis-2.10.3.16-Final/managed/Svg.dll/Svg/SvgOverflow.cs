using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgOverflowConverter))]
public enum SvgOverflow
{
	Hidden,
	Inherit,
	Auto,
	Visible,
	Scroll
}

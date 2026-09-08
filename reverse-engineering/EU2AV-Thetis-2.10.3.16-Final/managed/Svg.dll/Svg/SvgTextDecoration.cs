using System;
using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgTextDecorationConverter))]
[Flags]
public enum SvgTextDecoration
{
	Inherit = 0,
	None = 1,
	Underline = 2,
	Overline = 4,
	LineThrough = 8,
	Blink = 0x10
}

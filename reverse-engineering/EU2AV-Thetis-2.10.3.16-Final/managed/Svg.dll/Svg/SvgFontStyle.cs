using System;
using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgFontStyleConverter))]
[Flags]
public enum SvgFontStyle
{
	Inherit = 0,
	Normal = 1,
	Oblique = 2,
	Italic = 4,
	All = Normal | Oblique | Italic
}

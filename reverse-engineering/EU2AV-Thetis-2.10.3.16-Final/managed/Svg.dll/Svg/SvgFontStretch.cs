using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgFontStretchConverter))]
public enum SvgFontStretch
{
	Normal,
	Wider,
	Narrower,
	UltraCondensed,
	ExtraCondensed,
	Condensed,
	SemiCondensed,
	SemiExpanded,
	Expanded,
	ExtraExpanded,
	UltraExpanded,
	Inherit
}

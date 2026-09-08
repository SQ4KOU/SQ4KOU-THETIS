using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgTextLengthAdjustConverter))]
public enum SvgTextLengthAdjust
{
	Spacing,
	SpacingAndGlyphs
}

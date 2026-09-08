using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgStrokeLineCapConverter))]
public enum SvgStrokeLineCap
{
	Inherit,
	Butt,
	Round,
	Square
}

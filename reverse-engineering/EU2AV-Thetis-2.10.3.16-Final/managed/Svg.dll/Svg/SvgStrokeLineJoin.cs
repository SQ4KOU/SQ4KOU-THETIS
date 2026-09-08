using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgStrokeLineJoinConverter))]
public enum SvgStrokeLineJoin
{
	Inherit,
	Miter,
	MiterClip,
	Round,
	Bevel,
	Arcs
}

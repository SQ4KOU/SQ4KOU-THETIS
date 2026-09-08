using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgShapeRenderingConverter))]
public enum SvgShapeRendering
{
	Inherit,
	Auto,
	OptimizeSpeed,
	CrispEdges,
	GeometricPrecision
}

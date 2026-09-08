using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgTextRenderingConverter))]
public enum SvgTextRendering
{
	Inherit,
	Auto,
	OptimizeSpeed,
	OptimizeLegibility,
	GeometricPrecision
}

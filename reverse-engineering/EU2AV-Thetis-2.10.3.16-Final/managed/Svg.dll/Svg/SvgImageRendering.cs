using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgImageRenderingConverter))]
public enum SvgImageRendering
{
	Inherit,
	Auto,
	OptimizeSpeed,
	OptimizeQuality
}

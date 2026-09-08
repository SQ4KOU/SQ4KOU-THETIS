using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgGradientSpreadMethodConverter))]
public enum SvgGradientSpreadMethod
{
	Pad,
	Reflect,
	Repeat
}

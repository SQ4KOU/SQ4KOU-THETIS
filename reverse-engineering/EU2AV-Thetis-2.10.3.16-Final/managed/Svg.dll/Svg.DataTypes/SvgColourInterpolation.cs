using System.ComponentModel;

namespace Svg.DataTypes;

[TypeConverter(typeof(SvgColourInterpolationConverter))]
public enum SvgColourInterpolation
{
	Auto,
	SRGB,
	LinearRGB,
	Inherit
}

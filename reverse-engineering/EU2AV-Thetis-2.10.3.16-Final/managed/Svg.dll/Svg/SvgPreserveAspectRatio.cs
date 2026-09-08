using System.ComponentModel;
using Svg.DataTypes;

namespace Svg;

[TypeConverter(typeof(SvgPreserveAspectRatioConverter))]
public enum SvgPreserveAspectRatio
{
	xMidYMid,
	none,
	xMinYMin,
	xMidYMin,
	xMaxYMin,
	xMinYMid,
	xMaxYMid,
	xMinYMax,
	xMidYMax,
	xMaxYMax
}

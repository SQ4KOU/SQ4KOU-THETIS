using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgCoordinateUnitsConverter))]
public enum SvgCoordinateUnits
{
	ObjectBoundingBox,
	UserSpaceOnUse
}

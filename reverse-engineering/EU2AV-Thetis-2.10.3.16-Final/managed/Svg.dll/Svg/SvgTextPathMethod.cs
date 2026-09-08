using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(SvgTextPathMethodConverter))]
public enum SvgTextPathMethod
{
	Align,
	Stretch
}

using System.ComponentModel;

namespace Svg;

[TypeConverter(typeof(XmlSpaceHandlingConverter))]
public enum XmlSpaceHandling
{
	Default,
	Inherit,
	Preserve
}

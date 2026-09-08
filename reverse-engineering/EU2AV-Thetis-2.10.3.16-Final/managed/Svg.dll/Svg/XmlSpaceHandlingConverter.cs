namespace Svg;

public sealed class XmlSpaceHandlingConverter : EnumBaseConverter<XmlSpaceHandling>
{
	public XmlSpaceHandlingConverter()
		: base(CaseHandling.LowerCase)
	{
	}
}

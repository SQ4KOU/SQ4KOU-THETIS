namespace Svg;

public sealed class SvgTextPathMethodConverter : EnumBaseConverter<SvgTextPathMethod>
{
	public SvgTextPathMethodConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

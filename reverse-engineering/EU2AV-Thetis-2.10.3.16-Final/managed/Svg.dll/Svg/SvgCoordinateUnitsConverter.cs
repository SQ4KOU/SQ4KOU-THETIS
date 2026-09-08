namespace Svg;

public sealed class SvgCoordinateUnitsConverter : EnumBaseConverter<SvgCoordinateUnits>
{
	public SvgCoordinateUnitsConverter()
		: base(CaseHandling.CamelCase)
	{
	}
}

namespace ExCSS;

internal sealed class DevicePixelRatioFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.NaturalNumberConverter;

	public DevicePixelRatioFeature(string name)
		: base(name)
	{
	}
}

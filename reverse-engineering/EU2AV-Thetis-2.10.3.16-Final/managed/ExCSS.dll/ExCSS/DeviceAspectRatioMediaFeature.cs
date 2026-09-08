namespace ExCSS;

internal sealed class DeviceAspectRatioMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.RatioConverter;

	public DeviceAspectRatioMediaFeature(string name)
		: base(name)
	{
	}
}

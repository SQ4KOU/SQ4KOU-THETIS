namespace ExCSS;

internal sealed class DeviceHeightMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.LengthConverter;

	public DeviceHeightMediaFeature(string name)
		: base(name)
	{
	}
}

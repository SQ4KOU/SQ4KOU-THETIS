namespace ExCSS;

internal sealed class DeviceWidthMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.LengthConverter;

	public DeviceWidthMediaFeature(string name)
		: base(name)
	{
	}
}

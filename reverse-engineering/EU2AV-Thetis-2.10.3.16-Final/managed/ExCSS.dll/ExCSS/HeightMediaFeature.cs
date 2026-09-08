namespace ExCSS;

internal sealed class HeightMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.LengthConverter;

	public HeightMediaFeature(string name)
		: base(name)
	{
	}
}

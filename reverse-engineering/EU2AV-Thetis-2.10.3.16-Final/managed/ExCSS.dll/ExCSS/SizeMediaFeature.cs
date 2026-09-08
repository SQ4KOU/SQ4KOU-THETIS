namespace ExCSS;

internal sealed class SizeMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.LengthConverter;

	public SizeMediaFeature(string name)
		: base(name)
	{
	}
}

namespace ExCSS;

internal sealed class WidthMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.LengthConverter;

	public WidthMediaFeature(string name)
		: base(name)
	{
	}
}

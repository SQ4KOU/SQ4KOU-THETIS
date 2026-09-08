namespace ExCSS;

internal sealed class AspectRatioMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.RatioConverter;

	public AspectRatioMediaFeature(string name)
		: base(name)
	{
	}
}

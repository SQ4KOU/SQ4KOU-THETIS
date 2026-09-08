namespace ExCSS;

internal sealed class ResolutionMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.ResolutionConverter;

	public ResolutionMediaFeature(string name)
		: base(name)
	{
	}
}

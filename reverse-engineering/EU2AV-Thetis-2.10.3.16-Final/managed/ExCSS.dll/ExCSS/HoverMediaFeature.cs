namespace ExCSS;

internal sealed class HoverMediaFeature : MediaFeature
{
	private static readonly IValueConverter TheConverter = Map.HoverAbilities.ToConverter();

	internal override IValueConverter Converter => TheConverter;

	public HoverMediaFeature()
		: base(FeatureNames.Hover)
	{
	}
}

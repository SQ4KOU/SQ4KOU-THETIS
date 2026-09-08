namespace ExCSS;

internal sealed class PointerMediaFeature : MediaFeature
{
	private static readonly IValueConverter TheConverter = Map.PointerAccuracies.ToConverter();

	internal override IValueConverter Converter => TheConverter;

	public PointerMediaFeature()
		: base(FeatureNames.Pointer)
	{
	}
}

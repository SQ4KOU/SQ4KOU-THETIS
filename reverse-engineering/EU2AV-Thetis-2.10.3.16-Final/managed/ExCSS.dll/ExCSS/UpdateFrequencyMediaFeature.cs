namespace ExCSS;

internal sealed class UpdateFrequencyMediaFeature : MediaFeature
{
	private static readonly IValueConverter TheConverter = Map.UpdateFrequencies.ToConverter();

	internal override IValueConverter Converter => TheConverter;

	public UpdateFrequencyMediaFeature()
		: base(FeatureNames.UpdateFrequency)
	{
	}
}

namespace ExCSS;

internal sealed class FeatureProperty : Property
{
	internal override IValueConverter Converter => Feature.Converter;

	internal MediaFeature Feature { get; }

	internal FeatureProperty(MediaFeature feature)
		: base(feature.Name)
	{
		Feature = feature;
	}
}

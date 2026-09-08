namespace ExCSS;

internal sealed class OrientationMediaFeature : MediaFeature
{
	private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.Portrait, Keywords.Landscape);

	internal override IValueConverter Converter => TheConverter;

	public OrientationMediaFeature()
		: base(FeatureNames.Orientation)
	{
	}
}

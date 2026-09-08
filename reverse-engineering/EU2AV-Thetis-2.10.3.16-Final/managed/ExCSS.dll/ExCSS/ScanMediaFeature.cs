namespace ExCSS;

internal sealed class ScanMediaFeature : MediaFeature
{
	private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.Interlace, Keywords.Progressive);

	internal override IValueConverter Converter => TheConverter;

	public ScanMediaFeature()
		: base(FeatureNames.Scan)
	{
	}
}

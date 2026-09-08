namespace ExCSS;

internal sealed class GridMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.BinaryConverter;

	public GridMediaFeature()
		: base(FeatureNames.Grid)
	{
	}
}

namespace ExCSS;

internal sealed class MonochromeMediaFeature : MediaFeature
{
	internal override IValueConverter Converter
	{
		get
		{
			if (!base.IsMinimum && !base.IsMaximum)
			{
				return Converters.NaturalIntegerConverter.Option(1);
			}
			return Converters.NaturalIntegerConverter;
		}
	}

	public MonochromeMediaFeature(string name)
		: base(name)
	{
	}
}

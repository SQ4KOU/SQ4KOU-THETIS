namespace ExCSS;

internal sealed class ColorIndexMediaFeature : MediaFeature
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

	public ColorIndexMediaFeature(string name)
		: base(name)
	{
	}
}

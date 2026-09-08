namespace ExCSS;

internal sealed class ColorMediaFeature : MediaFeature
{
	internal override IValueConverter Converter
	{
		get
		{
			if (!base.IsMinimum && !base.IsMaximum)
			{
				return Converters.PositiveIntegerConverter.Option(1);
			}
			return Converters.PositiveIntegerConverter;
		}
	}

	public ColorMediaFeature(string name)
		: base(name)
	{
	}
}

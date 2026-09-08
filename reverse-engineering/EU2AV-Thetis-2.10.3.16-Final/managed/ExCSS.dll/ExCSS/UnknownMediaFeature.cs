namespace ExCSS;

internal sealed class UnknownMediaFeature : MediaFeature
{
	internal override IValueConverter Converter => Converters.Any;

	public UnknownMediaFeature(string name)
		: base(name)
	{
	}
}

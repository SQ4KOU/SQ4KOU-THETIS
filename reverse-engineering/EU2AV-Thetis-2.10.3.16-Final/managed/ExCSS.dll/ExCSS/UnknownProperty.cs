namespace ExCSS;

internal sealed class UnknownProperty : Property
{
	internal override IValueConverter Converter => Converters.Any;

	internal UnknownProperty(string name)
		: base(name)
	{
	}
}

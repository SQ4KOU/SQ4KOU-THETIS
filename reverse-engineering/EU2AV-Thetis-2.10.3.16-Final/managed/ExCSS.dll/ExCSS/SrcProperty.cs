namespace ExCSS;

internal sealed class SrcProperty : Property
{
	internal override IValueConverter Converter => Converters.Any;

	public SrcProperty()
		: base(PropertyNames.Src)
	{
	}
}

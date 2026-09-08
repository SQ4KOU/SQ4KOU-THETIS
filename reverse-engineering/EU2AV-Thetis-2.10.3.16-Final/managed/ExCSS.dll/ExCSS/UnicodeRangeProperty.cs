namespace ExCSS;

internal sealed class UnicodeRangeProperty : Property
{
	internal override IValueConverter Converter => Converters.Any;

	public UnicodeRangeProperty()
		: base(PropertyNames.UnicodeRange)
	{
	}
}

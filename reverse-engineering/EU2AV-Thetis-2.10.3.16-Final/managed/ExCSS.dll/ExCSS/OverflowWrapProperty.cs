namespace ExCSS;

internal sealed class OverflowWrapProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OverflowWrapConverter;

	internal override IValueConverter Converter => StyleConverter;

	public OverflowWrapProperty()
		: base(PropertyNames.OverflowWrap)
	{
	}
}

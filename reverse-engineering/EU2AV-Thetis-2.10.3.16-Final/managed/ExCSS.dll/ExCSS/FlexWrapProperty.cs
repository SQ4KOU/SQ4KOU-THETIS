namespace ExCSS;

internal sealed class FlexWrapProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FlexWrapConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FlexWrapProperty()
		: base(PropertyNames.FlexWrap)
	{
	}
}

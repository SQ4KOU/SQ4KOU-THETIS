namespace ExCSS;

internal sealed class FlexDirectionProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FlexDirectionConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FlexDirectionProperty()
		: base(PropertyNames.FlexDirection)
	{
	}
}

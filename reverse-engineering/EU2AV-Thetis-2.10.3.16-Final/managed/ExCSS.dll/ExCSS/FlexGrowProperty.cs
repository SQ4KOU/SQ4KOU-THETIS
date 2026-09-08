namespace ExCSS;

internal sealed class FlexGrowProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FlexGrowShrinkConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FlexGrowProperty()
		: base(PropertyNames.FlexGrow)
	{
	}
}

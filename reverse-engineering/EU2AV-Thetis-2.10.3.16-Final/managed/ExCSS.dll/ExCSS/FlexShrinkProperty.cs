namespace ExCSS;

internal sealed class FlexShrinkProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FlexGrowShrinkConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FlexShrinkProperty()
		: base(PropertyNames.FlexShrink)
	{
	}
}

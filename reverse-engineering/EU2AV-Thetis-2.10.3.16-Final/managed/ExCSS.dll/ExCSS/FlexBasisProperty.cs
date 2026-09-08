namespace ExCSS;

internal sealed class FlexBasisProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FlexBasisConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FlexBasisProperty()
		: base(PropertyNames.FlexBasis)
	{
	}
}

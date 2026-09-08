namespace ExCSS;

internal sealed class AlignSelfProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.AlignSelfConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal AlignSelfProperty()
		: base(PropertyNames.AlignSelf)
	{
	}
}

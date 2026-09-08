namespace ExCSS;

internal sealed class StrokeDasharrayProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.StrokeDasharrayConverter;

	internal override IValueConverter Converter => StyleConverter;

	public StrokeDasharrayProperty()
		: base(PropertyNames.StrokeDasharray, PropertyFlags.Unitless | PropertyFlags.Animatable)
	{
	}
}

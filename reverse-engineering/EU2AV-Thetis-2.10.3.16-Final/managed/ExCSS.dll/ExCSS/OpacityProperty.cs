namespace ExCSS;

internal sealed class OpacityProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.OptionalPercentOrNumberConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal OpacityProperty()
		: base(PropertyNames.Opacity, PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class StrokeDashoffsetProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter;

	internal override IValueConverter Converter => StyleConverter;

	public StrokeDashoffsetProperty()
		: base(PropertyNames.StrokeDashoffset, PropertyFlags.Animatable)
	{
	}
}

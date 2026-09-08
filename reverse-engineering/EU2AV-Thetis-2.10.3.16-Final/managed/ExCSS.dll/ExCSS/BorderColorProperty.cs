namespace ExCSS;

internal sealed class BorderColorProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.Periodic(PropertyNames.BorderTopColor, PropertyNames.BorderRightColor, PropertyNames.BorderBottomColor, PropertyNames.BorderLeftColor).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderColorProperty()
		: base(PropertyNames.BorderColor, PropertyFlags.Hashless | PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class BackgroundColorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BackgroundColorProperty()
		: base(PropertyNames.BackgroundColor, PropertyFlags.Hashless | PropertyFlags.Animatable)
	{
	}
}

namespace ExCSS;

internal sealed class BackfaceVisibilityProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.BackfaceVisibilityConverter.OrDefault(value: true);

	internal override IValueConverter Converter => StyleConverter;

	internal BackfaceVisibilityProperty()
		: base(PropertyNames.BackfaceVisibility)
	{
	}
}

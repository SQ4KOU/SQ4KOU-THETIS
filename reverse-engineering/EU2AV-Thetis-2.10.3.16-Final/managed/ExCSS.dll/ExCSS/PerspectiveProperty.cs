namespace ExCSS;

internal sealed class PerspectiveProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LengthConverter.OrNone().OrDefault(Length.Zero);

	internal override IValueConverter Converter => StyleConverter;

	internal PerspectiveProperty()
		: base(PropertyNames.Perspective, PropertyFlags.Animatable)
	{
	}
}

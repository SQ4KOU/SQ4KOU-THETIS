namespace ExCSS;

internal sealed class TransformProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.TransformConverter.Many().OrNone().OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal TransformProperty()
		: base(PropertyNames.Transform, PropertyFlags.Animatable)
	{
	}
}

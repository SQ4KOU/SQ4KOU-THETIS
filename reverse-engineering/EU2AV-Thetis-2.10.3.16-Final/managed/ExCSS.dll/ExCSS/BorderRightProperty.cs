namespace ExCSS;

internal sealed class BorderRightProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.LineWidthConverter.Option().For(PropertyNames.BorderRightWidth), Converters.LineStyleConverter.Option().For(PropertyNames.BorderRightStyle), Converters.CurrentColorConverter.Option().For(PropertyNames.BorderRightColor)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderRightProperty()
		: base(PropertyNames.BorderRight, PropertyFlags.Animatable)
	{
	}
}

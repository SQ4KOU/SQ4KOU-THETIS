namespace ExCSS;

internal sealed class OutlineProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.LineWidthConverter.Option().For(PropertyNames.OutlineWidth), Converters.LineStyleConverter.Option().For(PropertyNames.OutlineStyle), Converters.InvertedColorConverter.Option().For(PropertyNames.OutlineColor)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal OutlineProperty()
		: base(PropertyNames.Outline, PropertyFlags.Animatable)
	{
	}
}

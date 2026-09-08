namespace ExCSS;

internal sealed class BorderLeftProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.LineWidthConverter.Option().For(PropertyNames.BorderLeftWidth), Converters.LineStyleConverter.Option().For(PropertyNames.BorderLeftStyle), Converters.CurrentColorConverter.Option().For(PropertyNames.BorderLeftColor)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal BorderLeftProperty()
		: base(PropertyNames.BorderLeft, PropertyFlags.Animatable)
	{
	}
}

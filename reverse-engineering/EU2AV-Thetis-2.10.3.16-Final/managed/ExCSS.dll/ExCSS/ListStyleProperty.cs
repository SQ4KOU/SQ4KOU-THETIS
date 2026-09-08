namespace ExCSS;

internal sealed class ListStyleProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.WithAny(Converters.ListStyleConverter.Option().For(PropertyNames.ListStyleType), Converters.ListPositionConverter.Option().For(PropertyNames.ListStylePosition), Converters.OptionalImageSourceConverter.Option().For(PropertyNames.ListStyleImage)).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ListStyleProperty()
		: base(PropertyNames.ListStyle, PropertyFlags.Inherited)
	{
	}
}

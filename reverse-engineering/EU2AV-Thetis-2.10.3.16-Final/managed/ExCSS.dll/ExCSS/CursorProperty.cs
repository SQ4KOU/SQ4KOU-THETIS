namespace ExCSS;

internal sealed class CursorProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ImageSourceConverter.Or(Converters.WithOrder(Converters.ImageSourceConverter.Required(), Converters.NumberConverter.Required(), Converters.NumberConverter.Required())).RequiresEnd(Map.Cursors.ToConverter()).OrDefault(SystemCursor.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal CursorProperty()
		: base(PropertyNames.Cursor, PropertyFlags.Inherited)
	{
	}
}

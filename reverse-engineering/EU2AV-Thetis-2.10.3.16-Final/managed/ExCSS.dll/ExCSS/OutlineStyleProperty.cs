namespace ExCSS;

internal sealed class OutlineStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

	internal override IValueConverter Converter => StyleConverter;

	internal OutlineStyleProperty()
		: base(PropertyNames.OutlineStyle)
	{
	}
}

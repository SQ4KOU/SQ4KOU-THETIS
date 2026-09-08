namespace ExCSS;

internal sealed class BackgroundRepeatProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.BackgroundRepeatsConverter.FromList().OrDefault(BackgroundRepeat.Repeat);

	internal override IValueConverter Converter => ListConverter;

	internal BackgroundRepeatProperty()
		: base(PropertyNames.BackgroundRepeat)
	{
	}
}

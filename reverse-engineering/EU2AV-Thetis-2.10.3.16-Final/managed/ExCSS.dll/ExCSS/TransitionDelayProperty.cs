namespace ExCSS;

internal sealed class TransitionDelayProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.TimeConverter.FromList().OrDefault(Time.Zero);

	internal override IValueConverter Converter => ListConverter;

	internal TransitionDelayProperty()
		: base(PropertyNames.TransitionDelay)
	{
	}
}

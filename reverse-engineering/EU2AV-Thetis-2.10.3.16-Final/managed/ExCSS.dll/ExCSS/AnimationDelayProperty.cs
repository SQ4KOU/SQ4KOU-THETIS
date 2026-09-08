namespace ExCSS;

internal sealed class AnimationDelayProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.TimeConverter.FromList().OrDefault(Time.Zero);

	internal override IValueConverter Converter => ListConverter;

	internal AnimationDelayProperty()
		: base(PropertyNames.AnimationDelay)
	{
	}
}

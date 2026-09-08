namespace ExCSS;

internal sealed class AnimationDurationProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.TimeConverter.FromList().OrDefault(Time.Zero);

	internal override IValueConverter Converter => ListConverter;

	internal AnimationDurationProperty()
		: base(PropertyNames.AnimationDuration)
	{
	}
}

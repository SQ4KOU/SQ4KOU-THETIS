namespace ExCSS;

internal sealed class TransitionProperty : ShorthandProperty
{
	internal static readonly IValueConverter ListConverter = Converters.WithAny(Converters.AnimatableConverter.Option().For(PropertyNames.TransitionProperty), Converters.TimeConverter.Option().For(PropertyNames.TransitionDuration), Converters.TransitionConverter.Option().For(PropertyNames.TransitionTimingFunction), Converters.TimeConverter.Option().For(PropertyNames.TransitionDelay)).FromList().OrDefault();

	internal override IValueConverter Converter => ListConverter;

	internal TransitionProperty()
		: base(PropertyNames.Transition)
	{
	}
}

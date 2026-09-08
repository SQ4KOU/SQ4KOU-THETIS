namespace ExCSS;

internal sealed class AnimationProperty : ShorthandProperty
{
	private static readonly IValueConverter ListConverter = Converters.WithAny(Converters.TimeConverter.Option().For(PropertyNames.AnimationDuration), Converters.TransitionConverter.Option().For(PropertyNames.AnimationTimingFunction), Converters.TimeConverter.Option().For(PropertyNames.AnimationDelay), Converters.PositiveOrInfiniteNumberConverter.Option().For(PropertyNames.AnimationIterationCount), Converters.AnimationDirectionConverter.Option().For(PropertyNames.AnimationDirection), Converters.AnimationFillStyleConverter.Option().For(PropertyNames.AnimationFillMode), Converters.PlayStateConverter.Option().For(PropertyNames.AnimationPlayState), Converters.IdentifierConverter.Option().For(PropertyNames.AnimationName)).FromList().OrDefault();

	internal override IValueConverter Converter => ListConverter;

	internal AnimationProperty()
		: base(PropertyNames.Animation)
	{
	}
}

namespace ExCSS;

internal sealed class AnimationTimingFunctionProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.TransitionConverter.FromList().OrDefault(Map.TimingFunctions[Keywords.Ease]);

	internal override IValueConverter Converter => ListConverter;

	internal AnimationTimingFunctionProperty()
		: base(PropertyNames.AnimationTimingFunction)
	{
	}
}

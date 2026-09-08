namespace ExCSS;

internal sealed class AnimationIterationCountProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.PositiveOrInfiniteNumberConverter.FromList().OrDefault(1f);

	internal override IValueConverter Converter => ListConverter;

	internal AnimationIterationCountProperty()
		: base(PropertyNames.AnimationIterationCount)
	{
	}
}

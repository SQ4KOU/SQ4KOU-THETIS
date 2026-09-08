namespace ExCSS;

internal sealed class AnimationFillModeProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.AnimationFillStyleConverter.FromList().OrDefault(AnimationFillStyle.None);

	internal override IValueConverter Converter => ListConverter;

	internal AnimationFillModeProperty()
		: base(PropertyNames.AnimationFillMode)
	{
	}
}

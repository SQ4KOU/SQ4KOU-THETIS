namespace ExCSS;

internal sealed class AnimationDirectionProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.AnimationDirectionConverter.FromList().OrDefault(AnimationDirection.Normal);

	internal override IValueConverter Converter => ListConverter;

	internal AnimationDirectionProperty()
		: base(PropertyNames.AnimationDirection)
	{
	}
}

namespace ExCSS;

internal sealed class AnimationNameProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.IdentifierConverter.FromList().OrNone().OrDefault();

	internal override IValueConverter Converter => ListConverter;

	internal AnimationNameProperty()
		: base(PropertyNames.AnimationName)
	{
	}
}

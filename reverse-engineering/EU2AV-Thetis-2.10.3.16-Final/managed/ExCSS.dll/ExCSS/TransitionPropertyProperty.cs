namespace ExCSS;

internal sealed class TransitionPropertyProperty : Property
{
	private static readonly IValueConverter ListConverter = Converters.AnimatableConverter.FromList().OrNone().OrDefault(Keywords.All);

	internal override IValueConverter Converter => ListConverter;

	internal TransitionPropertyProperty()
		: base(PropertyNames.TransitionProperty)
	{
	}
}

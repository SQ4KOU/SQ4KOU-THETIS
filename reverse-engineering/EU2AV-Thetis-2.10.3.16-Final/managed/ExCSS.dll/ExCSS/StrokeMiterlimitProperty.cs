namespace ExCSS;

internal sealed class StrokeMiterlimitProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.StrokeMiterlimitConverter;

	internal override IValueConverter Converter => StyleConverter;

	public StrokeMiterlimitProperty()
		: base(PropertyNames.StrokeMiterlimit, PropertyFlags.Animatable)
	{
	}
}

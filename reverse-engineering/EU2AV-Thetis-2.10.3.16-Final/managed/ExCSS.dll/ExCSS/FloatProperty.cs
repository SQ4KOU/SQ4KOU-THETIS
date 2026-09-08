namespace ExCSS;

internal sealed class FloatProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.FloatingConverter.OrDefault(Floating.None);

	internal override IValueConverter Converter => StyleConverter;

	internal FloatProperty()
		: base(PropertyNames.Float)
	{
	}
}

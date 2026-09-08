namespace ExCSS;

internal sealed class ObjectFitProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ObjectFittingConverter.OrDefault(ObjectFitting.Fill);

	internal override IValueConverter Converter => StyleConverter;

	internal ObjectFitProperty()
		: base(PropertyNames.ObjectFit)
	{
	}
}

namespace ExCSS;

internal sealed class ClipProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.ShapeConverter.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ClipProperty()
		: base(PropertyNames.Clip, PropertyFlags.Animatable)
	{
	}
}

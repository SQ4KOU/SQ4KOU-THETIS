namespace ExCSS;

internal sealed class TransformStyleProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.Toggle(Keywords.Flat, Keywords.Preserve3d).OrDefault(value: true);

	internal override IValueConverter Converter => StyleConverter;

	internal TransformStyleProperty()
		: base(PropertyNames.TransformStyle)
	{
	}
}

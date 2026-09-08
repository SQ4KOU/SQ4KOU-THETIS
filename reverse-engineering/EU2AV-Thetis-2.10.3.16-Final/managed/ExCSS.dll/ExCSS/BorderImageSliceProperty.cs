namespace ExCSS;

internal sealed class BorderImageSliceProperty : Property
{
	internal static readonly IValueConverter TheConverter = Converters.WithAny(Converters.BorderSliceConverter.Option(new Length(100f, Length.Unit.Percent)), Converters.BorderSliceConverter.Option(), Converters.BorderSliceConverter.Option(), Converters.BorderSliceConverter.Option(), Converters.Assign(Keywords.Fill, result: true).Option(defaultValue: false));

	private static readonly IValueConverter StyleConverter = TheConverter.OrDefault(Length.Full);

	internal override IValueConverter Converter => StyleConverter;

	internal BorderImageSliceProperty()
		: base(PropertyNames.BorderImageSlice)
	{
	}
}

namespace ExCSS;

internal sealed class CounterResetProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.Continuous(Converters.WithOrder(Converters.IdentifierConverter.Required(), Converters.IntegerConverter.Option(0))).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal CounterResetProperty()
		: base(PropertyNames.CounterReset)
	{
	}
}

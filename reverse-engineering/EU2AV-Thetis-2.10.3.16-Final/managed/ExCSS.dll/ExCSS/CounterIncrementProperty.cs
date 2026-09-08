namespace ExCSS;

internal sealed class CounterIncrementProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.Continuous(Converters.WithOrder(Converters.IdentifierConverter.Required(), Converters.IntegerConverter.Option(1))).OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal CounterIncrementProperty()
		: base(PropertyNames.CounterIncrement)
	{
	}
}

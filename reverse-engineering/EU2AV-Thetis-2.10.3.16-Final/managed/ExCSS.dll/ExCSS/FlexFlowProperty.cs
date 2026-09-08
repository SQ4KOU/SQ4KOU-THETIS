namespace ExCSS;

internal sealed class FlexFlowProperty : ShorthandProperty
{
	private static readonly IValueConverter StyleConverter = Converters.FlexFlowConverter;

	internal override IValueConverter Converter => StyleConverter;

	internal FlexFlowProperty()
		: base(PropertyNames.FlexFlow)
	{
	}
}

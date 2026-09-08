namespace ExCSS;

internal sealed class PageBreakInsideProperty : Property
{
	private static readonly IValueConverter StyleConverter = Converters.Assign(Keywords.Auto, BreakMode.Auto).Or(Keywords.Avoid, BreakMode.Avoid).OrDefault(BreakMode.Auto);

	internal override IValueConverter Converter => StyleConverter;

	internal PageBreakInsideProperty()
		: base(PropertyNames.PageBreakInside)
	{
	}
}

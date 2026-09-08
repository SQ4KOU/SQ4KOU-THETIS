using System.Collections.Generic;

namespace ExCSS;

internal sealed class LinearGradientConverter : GradientConverter
{
	private readonly IValueConverter _converter;

	public LinearGradientConverter()
	{
		_converter = Converters.AngleConverter.Or(Converters.SideOrCornerConverter.StartsWithKeyword(Keywords.To));
	}

	protected override IPropertyValue ConvertFirstArgument(IEnumerable<Token> value)
	{
		return _converter.Convert(value);
	}
}

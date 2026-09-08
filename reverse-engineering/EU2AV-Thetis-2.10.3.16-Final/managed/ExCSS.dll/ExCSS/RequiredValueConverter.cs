using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class RequiredValueConverter : IValueConverter
{
	private readonly IValueConverter _converter;

	public RequiredValueConverter(IValueConverter converter)
	{
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		if (!value.Any())
		{
			return null;
		}
		return _converter.Convert(value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return _converter.Construct(properties);
	}
}

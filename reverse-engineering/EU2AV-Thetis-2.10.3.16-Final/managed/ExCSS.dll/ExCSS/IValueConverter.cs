using System.Collections.Generic;

namespace ExCSS;

internal interface IValueConverter
{
	IPropertyValue Convert(IEnumerable<Token> value);

	IPropertyValue Construct(Property[] properties);
}

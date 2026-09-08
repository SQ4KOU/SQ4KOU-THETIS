using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class FallbackValueConverter : IValueConverter
{
	private sealed class OptionValue : IPropertyValue
	{
		public string CssText => string.Empty;

		public TokenValue Original { get; }

		public OptionValue(IEnumerable<Token> tokens)
		{
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private readonly IValueConverter _converter;

	private readonly TokenValue _defaultValue;

	public FallbackValueConverter(IValueConverter converter, TokenValue defaultValue)
	{
		_converter = converter;
		_defaultValue = defaultValue;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		if (!value.Any())
		{
			return new OptionValue(_defaultValue);
		}
		return _converter.Convert(value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return _converter.Construct(properties) ?? new OptionValue(_defaultValue);
	}
}

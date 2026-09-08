using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class OptionValueConverter : IValueConverter
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
			return null;
		}
	}

	private readonly IValueConverter _converter;

	public OptionValueConverter(IValueConverter converter)
	{
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		if (!value.Any())
		{
			return new OptionValue(value);
		}
		return _converter.Convert(value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return _converter.Construct(properties) ?? new OptionValue(Enumerable.Empty<Token>());
	}
}
internal sealed class OptionValueConverter<T> : IValueConverter
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
			return null;
		}
	}

	private readonly IValueConverter _converter;

	public OptionValueConverter(IValueConverter converter)
	{
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		if (!value.Any())
		{
			return new OptionValue(value);
		}
		return _converter.Convert(value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return _converter.Construct(properties) ?? new OptionValue(Enumerable.Empty<Token>());
	}
}

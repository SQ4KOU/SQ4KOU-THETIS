using System.Collections.Generic;

namespace ExCSS;

internal sealed class DictionaryValueConverter<T> : IValueConverter
{
	private sealed class EnumeratedValue : IPropertyValue
	{
		public string CssText { get; }

		public TokenValue Original { get; }

		public EnumeratedValue(string identifier, IEnumerable<Token> tokens)
		{
			CssText = identifier;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private readonly Dictionary<string, T> _values;

	public DictionaryValueConverter(Dictionary<string, T> values)
	{
		_values = values;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		string text = value.ToIdentifier();
		if (text == null || !_values.TryGetValue(text, out var _))
		{
			return null;
		}
		return new EnumeratedValue(text, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<EnumeratedValue>();
	}
}

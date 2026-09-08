using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class ConditionalStartsWithValueConverter : IValueConverter
{
	private sealed class ConditionalStartValue : IPropertyValue
	{
		private readonly string _start;

		private readonly IPropertyValue _value;

		public string CssText
		{
			get
			{
				if (string.IsNullOrEmpty(_start))
				{
					return _value.CssText;
				}
				return _start + " " + _value.CssText;
			}
		}

		public TokenValue Original { get; }

		public ConditionalStartValue(string start, IPropertyValue value, IEnumerable<Token> tokens)
		{
			_start = start;
			_value = value;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return _value.ExtractFor(name);
		}
	}

	private readonly string _when;

	private readonly string[] _prefixKeywords;

	private readonly IValueConverter _converter;

	public ConditionalStartsWithValueConverter(string when, IValueConverter converter, params string[] prefixKeywords)
	{
		_when = when;
		_prefixKeywords = prefixKeywords;
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		IEnumerator<Token> enumerator = value.GetEnumerator();
		while (enumerator.MoveNext() && enumerator.Current.Type == TokenType.Whitespace)
		{
		}
		if (enumerator.Current.Type != TokenType.Ident || !_prefixKeywords.Contains(enumerator.Current.Data))
		{
			return null;
		}
		string data = enumerator.Current.Data;
		List<Token> list = new List<Token>();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Type != TokenType.Whitespace)
			{
				list.Add(enumerator.Current);
			}
		}
		IPropertyValue propertyValue = _converter.Convert(list);
		if (propertyValue != null && propertyValue.CssText != _when)
		{
			return null;
		}
		return new ConditionalStartValue(data, propertyValue, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		IPropertyValue propertyValue = _converter.Construct(properties);
		if (propertyValue == null)
		{
			return null;
		}
		return CreateFrom(propertyValue, Enumerable.Empty<Token>());
	}

	private IPropertyValue CreateFrom(IPropertyValue value, IEnumerable<Token> tokens)
	{
		if (value == null)
		{
			return null;
		}
		return new ConditionalStartValue(string.Empty, value, tokens);
	}
}

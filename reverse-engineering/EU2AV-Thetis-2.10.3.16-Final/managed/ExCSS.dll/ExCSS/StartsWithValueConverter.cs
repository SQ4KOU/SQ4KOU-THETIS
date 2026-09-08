using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class StartsWithValueConverter : IValueConverter
{
	private sealed class StartValue : IPropertyValue
	{
		private readonly string _start;

		private readonly IPropertyValue _value;

		public string CssText => _start + " " + _value.CssText;

		public TokenValue Original { get; }

		public StartValue(string start, IPropertyValue value, IEnumerable<Token> tokens)
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

	private readonly IValueConverter _converter;

	private readonly string _data;

	private readonly TokenType _type;

	public StartsWithValueConverter(TokenType type, string data, IValueConverter converter)
	{
		_type = type;
		_data = data;
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<Token> list = Transform(value);
		if (list == null)
		{
			return null;
		}
		return CreateFrom(_converter.Convert(list), value);
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
		return new StartValue(_data, value, tokens);
	}

	private List<Token> Transform(IEnumerable<Token> values)
	{
		using IEnumerator<Token> enumerator = values.GetEnumerator();
		while (enumerator.MoveNext() && enumerator.Current.Type == TokenType.Whitespace)
		{
		}
		if (enumerator.Current.Type != _type || !enumerator.Current.Data.Isi(_data))
		{
			return null;
		}
		List<Token> list = new List<Token>();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Type != TokenType.Whitespace || list.Count != 0)
			{
				list.Add(enumerator.Current);
			}
		}
		return list;
	}
}

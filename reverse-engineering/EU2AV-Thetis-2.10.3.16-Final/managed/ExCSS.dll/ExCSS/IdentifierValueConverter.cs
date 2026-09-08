using System;
using System.Collections.Generic;

namespace ExCSS;

internal sealed class IdentifierValueConverter : IValueConverter
{
	private sealed class IdentifierValue : IPropertyValue
	{
		public string CssText { get; }

		public TokenValue Original { get; }

		public IdentifierValue(string identifier, IEnumerable<Token> tokens)
		{
			CssText = identifier;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private readonly Func<IEnumerable<Token>, string> _converter;

	public IdentifierValueConverter(Func<IEnumerable<Token>, string> converter)
	{
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		string text = _converter(value);
		if (text == null)
		{
			return null;
		}
		return new IdentifierValue(text, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<IdentifierValue>();
	}
}
internal sealed class IdentifierValueConverter<T> : IValueConverter
{
	private sealed class IdentifierValue : IPropertyValue
	{
		public string CssText { get; }

		public TokenValue Original { get; }

		public IdentifierValue(string identifier, IEnumerable<Token> tokens)
		{
			CssText = identifier;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private readonly string _identifier;

	private readonly T _result;

	public IdentifierValueConverter(string identifier, T result)
	{
		_identifier = identifier;
		_result = result;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		if (!value.Is(_identifier))
		{
			return null;
		}
		return new IdentifierValue(_identifier, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<IdentifierValue>();
	}
}

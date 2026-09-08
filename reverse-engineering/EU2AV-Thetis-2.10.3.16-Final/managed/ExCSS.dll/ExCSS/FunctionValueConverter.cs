using System;
using System.Collections.Generic;

namespace ExCSS;

internal sealed class FunctionValueConverter : IValueConverter
{
	private sealed class FunctionValue : IPropertyValue
	{
		private readonly IPropertyValue _arguments;

		private readonly string _name;

		public string CssText => _name.StylesheetFunction(_arguments.CssText);

		public TokenValue Original { get; }

		public FunctionValue(string name, IPropertyValue arguments, IEnumerable<Token> tokens)
		{
			_name = name;
			_arguments = arguments;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private readonly IValueConverter _arguments;

	private readonly string _name;

	public FunctionValueConverter(string name, IValueConverter arguments)
	{
		_name = name;
		_arguments = arguments;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		FunctionToken functionToken = value.OnlyOrDefault() as FunctionToken;
		if (!Check(functionToken))
		{
			return null;
		}
		IPropertyValue propertyValue = _arguments.Convert(functionToken.ArgumentTokens);
		if (propertyValue == null)
		{
			return null;
		}
		return new FunctionValue(_name, propertyValue, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<FunctionValue>();
	}

	private bool Check(FunctionToken function)
	{
		return function?.Data.Equals(_name, StringComparison.OrdinalIgnoreCase) ?? false;
	}
}

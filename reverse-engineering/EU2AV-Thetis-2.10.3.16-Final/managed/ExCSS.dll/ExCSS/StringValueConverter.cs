using System.Collections.Generic;

namespace ExCSS;

internal sealed class StringValueConverter : IValueConverter
{
	private sealed class StringValue : IPropertyValue
	{
		private readonly string _value;

		public string CssText => _value.StylesheetString();

		public TokenValue Original { get; }

		public StringValue(string value, IEnumerable<Token> tokens)
		{
			_value = value;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		string text = value.ToCssString();
		if (text == null)
		{
			return null;
		}
		return new StringValue(text, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<StringValue>();
	}
}

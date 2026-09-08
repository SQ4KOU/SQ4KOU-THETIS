using System.Collections.Generic;

namespace ExCSS;

internal sealed class UrlValueConverter : IValueConverter
{
	private sealed class UrlValue : IPropertyValue
	{
		private readonly string _value;

		public string CssText => _value.StylesheetUrl();

		public TokenValue Original { get; }

		public UrlValue(string value, IEnumerable<Token> tokens)
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
		string text = value.ToUri();
		if (text == null)
		{
			return null;
		}
		return new UrlValue(text, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<UrlValue>();
	}
}

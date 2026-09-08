using System.Collections.Generic;

namespace ExCSS;

internal sealed class AnyValueConverter : IValueConverter
{
	private sealed class AnyValue : IPropertyValue
	{
		public string CssText => Original.ToText();

		public TokenValue Original { get; }

		public AnyValue(IEnumerable<Token> tokens)
		{
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		return new AnyValue(value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<AnyValue>();
	}
}

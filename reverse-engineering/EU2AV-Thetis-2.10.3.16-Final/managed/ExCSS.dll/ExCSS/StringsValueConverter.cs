using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class StringsValueConverter : IValueConverter
{
	private sealed class StringsValue : IPropertyValue
	{
		private readonly string[] _values;

		public string CssText => string.Join(" ", _values.Select((string m) => m.StylesheetString()));

		public TokenValue Original { get; }

		public StringsValue(string[] values, IEnumerable<Token> tokens)
		{
			_values = values;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<List<Token>> list = value.ToItems();
		int count = list.Count;
		if (count % 2 != 0)
		{
			return null;
		}
		string[] array = new string[list.Count];
		for (int i = 0; i < count; i++)
		{
			array[i] = list[i].ToCssString();
			if (array[i] == null)
			{
				return null;
			}
		}
		return new StringsValue(array, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<StringsValue>();
	}
}

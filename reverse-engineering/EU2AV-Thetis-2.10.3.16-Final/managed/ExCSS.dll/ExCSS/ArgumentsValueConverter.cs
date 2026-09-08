using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class ArgumentsValueConverter : IValueConverter
{
	private sealed class ArgumentsValue : IPropertyValue
	{
		private readonly IPropertyValue[] _arguments;

		public string CssText
		{
			get
			{
				IEnumerable<string> values = from m in _arguments
					where !string.IsNullOrEmpty(m.CssText)
					select m.CssText;
				return string.Join(", ", values);
			}
		}

		public TokenValue Original { get; }

		public ArgumentsValue(IPropertyValue[] arguments, IEnumerable<Token> tokens)
		{
			_arguments = arguments;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private readonly IValueConverter[] _converters;

	public ArgumentsValueConverter(params IValueConverter[] converters)
	{
		_converters = converters;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<List<Token>> list = value.ToList();
		int num = _converters.Length;
		if (list.Count > num)
		{
			return null;
		}
		IPropertyValue[] array = new IPropertyValue[num];
		for (int i = 0; i < num; i++)
		{
			IEnumerable<Token> enumerable;
			if (i >= list.Count)
			{
				enumerable = Enumerable.Empty<Token>();
			}
			else
			{
				IEnumerable<Token> enumerable2 = list[i];
				enumerable = enumerable2;
			}
			IEnumerable<Token> value2 = enumerable;
			array[i] = _converters[i].Convert(value2);
			if (array[i] == null)
			{
				return null;
			}
		}
		return new ArgumentsValue(array, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<ArgumentsValue>();
	}
}

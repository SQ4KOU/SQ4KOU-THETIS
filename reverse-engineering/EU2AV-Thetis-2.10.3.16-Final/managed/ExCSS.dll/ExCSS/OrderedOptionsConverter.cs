using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class OrderedOptionsConverter : IValueConverter
{
	private sealed class OptionsValue : IPropertyValue, IEnumerable<IPropertyValue>, IEnumerable
	{
		private readonly IPropertyValue[] _options;

		public string CssText => string.Join(" ", from m in _options
			where !string.IsNullOrEmpty(m.CssText)
			select m.CssText);

		public TokenValue Original { get; }

		public OptionsValue(IPropertyValue[] options, IEnumerable<Token> tokens)
		{
			_options = options;
			Original = new TokenValue(tokens);
		}

		IEnumerator<IPropertyValue> IEnumerable<IPropertyValue>.GetEnumerator()
		{
			return ((IEnumerable<IPropertyValue>)_options).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _options.GetEnumerator();
		}

		public TokenValue ExtractFor(string name)
		{
			List<Token> list = new List<Token>();
			IPropertyValue[] options = _options;
			for (int i = 0; i < options.Length; i++)
			{
				TokenValue tokenValue = options[i].ExtractFor(name);
				if (tokenValue != null && tokenValue.Count > 0)
				{
					if (list.Count > 0)
					{
						list.Add(Token.Whitespace);
					}
					list.AddRange(tokenValue);
				}
			}
			return new TokenValue(list);
		}
	}

	private readonly IValueConverter[] _converters;

	public OrderedOptionsConverter(params IValueConverter[] converters)
	{
		_converters = converters;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<Token> list = new List<Token>(value);
		IPropertyValue[] array = new IPropertyValue[_converters.Length];
		for (int i = 0; i < _converters.Length; i++)
		{
			array[i] = _converters[i].VaryStart(list);
			if (array[i] == null)
			{
				return null;
			}
		}
		if (list.Count != 0)
		{
			return null;
		}
		return new OptionsValue(array, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		IPropertyValue propertyValue = properties.Guard<OptionsValue>();
		if (propertyValue == null)
		{
			IPropertyValue[] array = new IPropertyValue[_converters.Length];
			for (int i = 0; i < _converters.Length; i++)
			{
				IPropertyValue propertyValue2 = _converters[i].Construct(properties);
				if (propertyValue2 == null)
				{
					return null;
				}
				array[i] = propertyValue2;
			}
			propertyValue = new OptionsValue(array, Enumerable.Empty<Token>());
		}
		return propertyValue;
	}
}

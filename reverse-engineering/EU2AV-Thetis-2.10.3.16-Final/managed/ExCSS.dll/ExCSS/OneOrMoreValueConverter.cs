using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class OneOrMoreValueConverter : IValueConverter
{
	private sealed class MultipleValue : IPropertyValue
	{
		private readonly IPropertyValue[] _values;

		public string CssText => string.Join(" ", from m in _values
			where !string.IsNullOrEmpty(m.CssText)
			select m.CssText);

		public TokenValue Original { get; }

		public MultipleValue(IPropertyValue[] values, IEnumerable<Token> tokens)
		{
			_values = values;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			List<Token> list = new List<Token>();
			IPropertyValue[] values = _values;
			for (int i = 0; i < values.Length; i++)
			{
				TokenValue tokenValue = values[i].ExtractFor(name);
				if (tokenValue != null)
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

	private readonly IValueConverter _converter;

	private readonly int _maximum;

	private readonly int _minimum;

	public OneOrMoreValueConverter(IValueConverter converter, int minimum, int maximum)
	{
		_converter = converter;
		_minimum = minimum;
		_maximum = maximum;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<List<Token>> list = value.ToItems();
		int count = list.Count;
		if (count < _minimum || count > _maximum)
		{
			return null;
		}
		IPropertyValue[] array = new IPropertyValue[list.Count];
		for (int i = 0; i < count; i++)
		{
			array[i] = _converter.Convert(list[i]);
			if (array[i] == null)
			{
				return null;
			}
		}
		return new MultipleValue(array, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		IPropertyValue propertyValue = properties.Guard<MultipleValue>();
		if (propertyValue == null)
		{
			IPropertyValue[] array = new IPropertyValue[properties.Length];
			for (int i = 0; i < properties.Length; i++)
			{
				IPropertyValue propertyValue2 = _converter.Construct(new Property[1] { properties[i] });
				if (propertyValue2 == null)
				{
					return null;
				}
				array[i] = propertyValue2;
			}
			propertyValue = new MultipleValue(array, Enumerable.Empty<Token>());
		}
		return propertyValue;
	}
}

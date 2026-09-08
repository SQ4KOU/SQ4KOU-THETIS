using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class ListValueConverter : IValueConverter
{
	private sealed class ListValue : IPropertyValue
	{
		private readonly IPropertyValue[] _values;

		public string CssText => string.Join(", ", _values.Select((IPropertyValue m) => m.CssText));

		public TokenValue Original { get; }

		public ListValue(IPropertyValue[] values, IEnumerable<Token> tokens)
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
						list.Add(Token.Comma);
					}
					list.AddRange(tokenValue);
				}
			}
			return new TokenValue(list);
		}
	}

	private readonly IValueConverter _converter;

	public ListValueConverter(IValueConverter converter)
	{
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<List<Token>> list = value.ToList();
		IPropertyValue[] array = new IPropertyValue[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = _converter.Convert(list[i]);
			if (array[i] == null)
			{
				return null;
			}
		}
		if (array.Length == 1)
		{
			return array[0];
		}
		return new ListValue(array, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		IPropertyValue propertyValue = properties.Guard<ListValue>();
		if (propertyValue == null)
		{
			List<List<Token>>[] array = new List<List<Token>>[properties.Length];
			Property[] array2 = new Property[properties.Length];
			int num = 0;
			for (int i = 0; i < properties.Length; i++)
			{
				IPropertyValue declaredValue = properties[i].DeclaredValue;
				array[i] = ((declaredValue != null) ? declaredValue.Original.ToList() : new List<List<Token>>());
				array2[i] = PropertyFactory.Instance.CreateLonghand(properties[i].Name);
				num = Math.Max(num, array[i].Count);
			}
			IPropertyValue[] array3 = new IPropertyValue[num];
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < array2.Length; k++)
				{
					List<List<Token>> list = array[k];
					IEnumerable<Token> enumerable;
					if (list.Count <= j)
					{
						enumerable = Enumerable.Empty<Token>();
					}
					else
					{
						IEnumerable<Token> enumerable2 = list[j];
						enumerable = enumerable2;
					}
					IEnumerable<Token> tokens = enumerable;
					array2[k].TrySetValue(new TokenValue(tokens));
				}
				array3[j] = _converter.Construct(array2);
			}
			propertyValue = new ListValue(array3, Enumerable.Empty<Token>());
		}
		return propertyValue;
	}
}

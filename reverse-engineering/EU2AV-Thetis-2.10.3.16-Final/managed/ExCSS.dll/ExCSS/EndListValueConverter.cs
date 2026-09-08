using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class EndListValueConverter : IValueConverter
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
						list.Add(Token.Whitespace);
					}
					list.AddRange(tokenValue);
				}
			}
			return new TokenValue(list);
		}
	}

	private readonly IValueConverter _endConverter;

	private readonly IValueConverter _listConverter;

	public EndListValueConverter(IValueConverter listConverter, IValueConverter endConverter)
	{
		_listConverter = listConverter;
		_endConverter = endConverter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<List<Token>> list = value.ToList();
		int num = list.Count - 1;
		IPropertyValue[] array = new IPropertyValue[num + 1];
		for (int i = 0; i < num; i++)
		{
			array[i] = _listConverter.Convert(list[i]);
			if (array[i] == null)
			{
				return null;
			}
		}
		array[num] = _endConverter.Convert(list[num]);
		if (array[num] == null)
		{
			return null;
		}
		return new ListValue(array, value);
	}

	public IPropertyValue Construct(Property[] properties)
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
			IValueConverter valueConverter = ((j < num - 1) ? _listConverter : _endConverter);
			array3[j] = valueConverter.Construct(array2);
		}
		return new ListValue(array3, Enumerable.Empty<Token>());
	}
}

using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class PeriodicValueConverter : IValueConverter
{
	private sealed class PeriodicValue : IPropertyValue
	{
		private readonly string[] _labels;

		private readonly IPropertyValue[] _values;

		private string[] Values
		{
			get
			{
				int num = _values.Length;
				if ((num == 0 || num == 4) ? true : false)
				{
					string cssText = _values[0].CssText;
					string cssText2 = _values[1].CssText;
					string cssText3 = _values[2].CssText;
					string cssText4 = _values[3].CssText;
					if (cssText2.Is(cssText4))
					{
						if (cssText.Is(cssText3))
						{
							if (cssText2.Is(cssText))
							{
								return new string[1] { cssText };
							}
							return new string[2] { cssText, cssText2 };
						}
						return new string[3] { cssText, cssText2, cssText3 };
					}
					return new string[4] { cssText, cssText2, cssText3, cssText4 };
				}
				string cssText5 = _values[0].CssText;
				string cssText6 = _values[1].CssText;
				if (cssText5.Is(cssText6))
				{
					return new string[1] { cssText5 };
				}
				return new string[2] { cssText5, cssText6 };
			}
		}

		public string CssText => string.Join(" ", Values);

		public TokenValue Original { get; }

		public PeriodicValue(IPropertyValue[] options, IEnumerable<Token> tokens, string[] labels)
		{
			_values = new IPropertyValue[(labels.Length == 0) ? 4 : labels.Length];
			int num = _values.Length;
			if ((num == 0 || num == 4) ? true : false)
			{
				_values[0] = options[0];
				_values[1] = options[1] ?? _values[0];
				_values[2] = options[2] ?? _values[0];
				_values[3] = options[3] ?? _values[1];
			}
			else
			{
				_values[0] = options[0];
				_values[1] = options[1] ?? _values[0];
			}
			Original = new TokenValue(tokens);
			_labels = labels;
		}

		public TokenValue ExtractFor(string name)
		{
			if (name.Is(_labels[0]))
			{
				return _values[0].Original;
			}
			if (name.Is(_labels[1]))
			{
				return _values[1].Original;
			}
			if (_labels.Length == 4)
			{
				if (name.Is(_labels[2]))
				{
					return _values[2].Original;
				}
				if (name.Is(_labels[3]))
				{
					return _values[3].Original;
				}
			}
			return null;
		}
	}

	private readonly IValueConverter _converter;

	private readonly string[] _labels;

	public PeriodicValueConverter(IValueConverter converter, string[] labels)
	{
		_converter = converter;
		_labels = labels;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<Token> list = new List<Token>(value);
		IPropertyValue[] array = new IPropertyValue[(_labels.Length == 0) ? 4 : _labels.Length];
		if (list.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			array[i] = _converter.VaryStart(list);
			if (array[i] == null)
			{
				return null;
			}
		}
		if (list.Count != 0)
		{
			return null;
		}
		return new PeriodicValue(array, value, _labels);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		IPropertyValue[] array = new IPropertyValue[_labels.Length];
		int i;
		for (i = 0; i < _labels.Length; i++)
		{
			array[i] = _converter.Construct(properties.Where((Property m) => m.Name == _labels[i]).ToArray());
		}
		if (!array.All((IPropertyValue opt) => opt != null))
		{
			return null;
		}
		return new PeriodicValue(array, Enumerable.Empty<Token>(), _labels);
	}
}

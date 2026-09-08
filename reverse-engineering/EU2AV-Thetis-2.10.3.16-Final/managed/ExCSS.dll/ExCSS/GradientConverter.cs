using System.Collections.Generic;

namespace ExCSS;

internal abstract class GradientConverter : IValueConverter
{
	private sealed class StopValue : IPropertyValue
	{
		private readonly IPropertyValue _color;

		private readonly IPropertyValue _position;

		public string CssText
		{
			get
			{
				if (_color == null && _position != null)
				{
					return _position.CssText;
				}
				if (_color != null && _position == null)
				{
					return _color.CssText;
				}
				return _color?.CssText + " " + _position.CssText;
			}
		}

		public TokenValue Original { get; }

		public StopValue(IPropertyValue color, IPropertyValue position, IEnumerable<Token> tokens)
		{
			_color = color;
			_position = position;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private sealed class GradientValue : IPropertyValue
	{
		private readonly IPropertyValue _initial;

		private readonly IPropertyValue[] _stops;

		public string CssText
		{
			get
			{
				int num = _stops.Length;
				if (_initial != null)
				{
					num++;
				}
				string[] array = new string[num];
				num = 0;
				if (_initial != null)
				{
					array[num++] = _initial.CssText;
				}
				IPropertyValue[] stops = _stops;
				foreach (IPropertyValue propertyValue in stops)
				{
					array[num++] = propertyValue.CssText;
				}
				return string.Join(", ", array);
			}
		}

		public TokenValue Original { get; }

		public GradientValue(IPropertyValue initial, IPropertyValue[] stops, IEnumerable<Token> tokens)
		{
			_initial = initial;
			_stops = stops;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<List<Token>> list = value.ToList();
		IPropertyValue propertyValue = ((list.Count != 0) ? ConvertFirstArgument(list[0]) : null);
		int offset = ((propertyValue != null) ? 1 : 0);
		IPropertyValue[] array = ToGradientStops(list, offset);
		if (array == null)
		{
			return null;
		}
		return new GradientValue(propertyValue, array, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<GradientValue>();
	}

	private static IPropertyValue[] ToGradientStops(List<List<Token>> values, int offset)
	{
		IPropertyValue[] array = new IPropertyValue[values.Count - offset];
		int num = offset;
		int num2 = 0;
		while (num < values.Count)
		{
			array[num2] = ToGradientStop(values[num]);
			if (array[num2] == null)
			{
				return null;
			}
			num++;
			num2++;
		}
		return array;
	}

	private static IPropertyValue ToGradientStop(List<Token> value)
	{
		IPropertyValue propertyValue = null;
		IPropertyValue propertyValue2 = null;
		List<List<Token>> list = value.ToItems();
		if (list.Count != 0)
		{
			propertyValue2 = Converters.LengthOrPercentConverter.Convert(list[list.Count - 1]);
			if (propertyValue2 != null)
			{
				list.RemoveAt(list.Count - 1);
			}
		}
		if (list.Count != 0)
		{
			propertyValue = Converters.ColorConverter.Convert(list[list.Count - 1]);
			if (propertyValue != null)
			{
				list.RemoveAt(list.Count - 1);
			}
		}
		if (list.Count != 0)
		{
			return null;
		}
		return new StopValue(propertyValue, propertyValue2, value);
	}

	protected abstract IPropertyValue ConvertFirstArgument(IEnumerable<Token> value);
}

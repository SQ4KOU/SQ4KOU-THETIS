using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class ConstraintValueConverter : IValueConverter
{
	private sealed class TransformationValueConverter : IPropertyValue
	{
		private readonly string[] _labels;

		private readonly IPropertyValue _value;

		public string CssText => _value.CssText;

		public TokenValue Original => _value.Original;

		public TransformationValueConverter(IPropertyValue value, string[] labels)
		{
			_value = value;
			_labels = labels;
		}

		public TokenValue ExtractFor(string name)
		{
			if (!_labels.Contains(name))
			{
				return null;
			}
			return _value.ExtractFor(name);
		}
	}

	private readonly IValueConverter _converter;

	private readonly string[] _labels;

	public ConstraintValueConverter(IValueConverter converter, string[] labels)
	{
		_converter = converter;
		_labels = labels;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		IPropertyValue propertyValue = _converter.Convert(value);
		if (propertyValue == null)
		{
			return null;
		}
		return new TransformationValueConverter(propertyValue, _labels);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		IEnumerable<Property> enumerable = properties.Where((Property m) => _labels.Contains(m.Name));
		string text = null;
		foreach (Property item in enumerable)
		{
			string value = item.Value;
			if (text != null && value != text)
			{
				return null;
			}
			text = value;
		}
		IPropertyValue propertyValue = _converter.Construct(enumerable.Take(1).ToArray());
		if (propertyValue == null)
		{
			return null;
		}
		return new TransformationValueConverter(propertyValue, _labels);
	}
}

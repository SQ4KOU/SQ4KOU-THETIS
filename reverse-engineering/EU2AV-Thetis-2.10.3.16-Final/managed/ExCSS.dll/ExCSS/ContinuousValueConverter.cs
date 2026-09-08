using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class ContinuousValueConverter : IValueConverter
{
	private sealed class OptionsValue : IPropertyValue
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

		public TokenValue ExtractFor(string name)
		{
			List<Token> list = new List<Token>();
			IPropertyValue[] options = _options;
			for (int i = 0; i < options.Length; i++)
			{
				TokenValue tokenValue = options[i].ExtractFor(name);
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

	public ContinuousValueConverter(IValueConverter converter)
	{
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<Token> list = new List<Token>(value);
		List<IPropertyValue> list2 = new List<IPropertyValue>();
		if (list.Count <= 0)
		{
			return null;
		}
		while (list.Count != 0)
		{
			IPropertyValue propertyValue = _converter.VaryStart(list);
			if (propertyValue == null)
			{
				return null;
			}
			list2.Add(propertyValue);
		}
		return new OptionsValue(list2.ToArray(), value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<OptionsValue>();
	}
}

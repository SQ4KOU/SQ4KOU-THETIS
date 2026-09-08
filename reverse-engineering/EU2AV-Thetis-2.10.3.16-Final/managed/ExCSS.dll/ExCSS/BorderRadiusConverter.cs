using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class BorderRadiusConverter : IValueConverter
{
	private sealed class BorderRadiusValue : IPropertyValue
	{
		private readonly IPropertyValue _horizontal;

		private readonly IPropertyValue _vertical;

		public string CssText
		{
			get
			{
				string cssText = _horizontal.CssText;
				if (_vertical == null)
				{
					return cssText;
				}
				string cssText2 = _vertical.CssText;
				if (!(cssText != cssText2))
				{
					return cssText;
				}
				return cssText + " / " + cssText2;
			}
		}

		public TokenValue Original { get; }

		public BorderRadiusValue(IPropertyValue horizontal, IPropertyValue vertical, TokenValue original)
		{
			_horizontal = horizontal;
			_vertical = vertical;
			Original = original;
		}

		public TokenValue ExtractFor(string name)
		{
			TokenValue items = _horizontal.ExtractFor(name);
			return new TokenValue(Enumerable.Concat(second: _vertical.ExtractFor(name), first: items.Concat(Token.Whitespace)));
		}
	}

	private readonly IValueConverter _converter = Converters.LengthOrPercentConverter.Periodic(PropertyNames.BorderTopLeftRadius, PropertyNames.BorderTopRightRadius, PropertyNames.BorderBottomRightRadius, PropertyNames.BorderBottomLeftRadius);

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		List<Token> list = new List<Token>();
		List<Token> list2 = new List<Token>();
		List<Token> list3 = list;
		foreach (Token item in value)
		{
			if (item.Type == TokenType.Delim && item.Data.Is("/"))
			{
				if (list3 == list2)
				{
					return null;
				}
				list3 = list2;
			}
			else
			{
				list3.Add(item);
			}
		}
		IPropertyValue propertyValue = _converter.Convert(list);
		if (propertyValue == null)
		{
			return null;
		}
		IPropertyValue propertyValue2 = ((list3 == list2) ? _converter.Convert(list2) : propertyValue);
		if (propertyValue2 == null)
		{
			return null;
		}
		return new BorderRadiusValue(propertyValue, propertyValue2, new TokenValue(value));
	}

	public IPropertyValue Construct(Property[] properties)
	{
		if (properties.Length == 4)
		{
			List<Token> list = new List<Token>();
			List<Token> list2 = new List<Token>();
			List<Property> list3 = new List<Property>
			{
				properties.First((Property m) => m.Name.Is(PropertyNames.BorderTopLeftRadius)),
				properties.First((Property m) => m.Name.Is(PropertyNames.BorderTopRightRadius)),
				properties.First((Property m) => m.Name.Is(PropertyNames.BorderBottomRightRadius)),
				properties.First((Property m) => m.Name.Is(PropertyNames.BorderBottomLeftRadius))
			};
			for (int num = 0; num < list3.Count; num++)
			{
				if (!(list3[num].DeclaredValue is IEnumerable<IPropertyValue> source))
				{
					return null;
				}
				TokenValue original = source.First().Original;
				TokenValue original2 = source.Last().Original;
				if (num != 0)
				{
					list.Add(Token.Whitespace);
					list2.Add(Token.Whitespace);
				}
				list.AddRange(original);
				list2.AddRange(original2);
			}
			IPropertyValue horizontal = _converter.Convert(list);
			IPropertyValue vertical = _converter.Convert(list2);
			IEnumerable<Token> tokens = list.Concat(new Token(TokenType.Delim, "/", TextPosition.Empty)).Concat(list2);
			return new BorderRadiusValue(horizontal, vertical, new TokenValue(tokens));
		}
		return null;
	}
}

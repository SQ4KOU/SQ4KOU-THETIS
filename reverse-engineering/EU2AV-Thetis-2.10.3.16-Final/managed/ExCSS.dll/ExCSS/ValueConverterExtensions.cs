using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal static class ValueConverterExtensions
{
	public static IPropertyValue ConvertDefault(this IValueConverter converter)
	{
		return converter.Convert(Enumerable.Empty<Token>());
	}

	public static IPropertyValue VaryStart(this IValueConverter converter, List<Token> list)
	{
		for (int num = list.Count; num > 0; num--)
		{
			if (list[num - 1].Type != TokenType.Whitespace)
			{
				IPropertyValue propertyValue = converter.Convert(list.Take(num));
				if (propertyValue != null)
				{
					list.RemoveRange(0, num);
					list.Trim();
					return propertyValue;
				}
			}
		}
		return converter.ConvertDefault();
	}

	public static IPropertyValue VaryAll(this IValueConverter converter, List<Token> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Type == TokenType.Whitespace)
			{
				continue;
			}
			for (int num = list.Count; num > i; num--)
			{
				int count = num - i;
				if (list[num - 1].Type != TokenType.Whitespace)
				{
					IPropertyValue propertyValue = converter.Convert(list.Skip(i).Take(count));
					if (propertyValue != null)
					{
						list.RemoveRange(i, count);
						list.Trim();
						return propertyValue;
					}
				}
			}
		}
		return converter.ConvertDefault();
	}

	public static IValueConverter Many(this IValueConverter converter, int min = 1, int max = 65535)
	{
		return new OneOrMoreValueConverter(converter, min, max);
	}

	public static IValueConverter FromList(this IValueConverter converter)
	{
		return new ListValueConverter(converter);
	}

	public static IValueConverter ToConverter<T>(this Dictionary<string, T> values)
	{
		return new DictionaryValueConverter<T>(values);
	}

	public static IValueConverter Periodic(this IValueConverter converter, params string[] labels)
	{
		return new PeriodicValueConverter(converter, labels);
	}

	public static IValueConverter RequiresEnd(this IValueConverter listConverter, IValueConverter endConverter)
	{
		return new EndListValueConverter(listConverter, endConverter);
	}

	public static IValueConverter Required(this IValueConverter converter)
	{
		return new RequiredValueConverter(converter);
	}

	public static IValueConverter Option(this IValueConverter converter)
	{
		return new OptionValueConverter(converter);
	}

	public static IValueConverter For(this IValueConverter converter, params string[] labels)
	{
		return new ConstraintValueConverter(converter, labels);
	}

	public static IValueConverter Option<T>(this IValueConverter converter, T defaultValue)
	{
		return new OptionValueConverter<T>(converter);
	}

	public static IValueConverter Or(this IValueConverter primary, IValueConverter secondary)
	{
		return new OrValueConverter(primary, secondary);
	}

	public static IValueConverter Or(this IValueConverter primary, string keyword)
	{
		return primary.Or<object>(keyword, null);
	}

	public static IValueConverter Or<T>(this IValueConverter primary, string keyword, T value)
	{
		IdentifierValueConverter<T> next = new IdentifierValueConverter<T>(keyword, value);
		return new OrValueConverter(primary, next);
	}

	public static IValueConverter OrNone(this IValueConverter primary)
	{
		return primary.Or(Keywords.None);
	}

	public static IValueConverter OrDefault(this IValueConverter primary)
	{
		return primary.OrInherit().Or(Keywords.Initial);
	}

	public static IValueConverter OrDefault<T>(this IValueConverter primary, T value)
	{
		return primary.OrInherit().Or(Keywords.Initial, value);
	}

	public static IValueConverter OrInherit(this IValueConverter primary)
	{
		return primary.Or(Keywords.Inherit);
	}

	public static IValueConverter OrAuto(this IValueConverter primary)
	{
		return primary.Or(Keywords.Auto);
	}

	public static IValueConverter OrGlobalValue(this IValueConverter primary)
	{
		return primary.OrInherit().Or(Keywords.Initial).Or(Keywords.Revert)
			.Or(Keywords.RevertLayer)
			.Or(Keywords.Unset);
	}

	public static IValueConverter ConditionalStartsWithKeyword(this IValueConverter primary, string when, params string[] keywords)
	{
		return new ConditionalStartsWithValueConverter(when, primary, keywords);
	}

	public static IValueConverter StartsWithKeyword(this IValueConverter converter, string keyword)
	{
		return new StartsWithValueConverter(TokenType.Ident, keyword, converter);
	}

	public static IValueConverter StartsWithDelimiter(this IValueConverter converter)
	{
		return new StartsWithValueConverter(TokenType.Delim, "/", converter);
	}

	public static IValueConverter WithCurrentColor(this IValueConverter converter)
	{
		return converter.Or(Keywords.CurrentColor, Color.Transparent);
	}

	public static IValueConverter WithFallback(this IValueConverter converter, int defaultValue)
	{
		return new FallbackValueConverter(converter, TokenValue.FromNumber(defaultValue));
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal static class ValueExtensions
{
	private static bool IsWeight(int value)
	{
		if (value != 100 && value != 200 && value != 300 && value != 400 && value != 500 && value != 600 && value != 700 && value != 800)
		{
			return value == 900;
		}
		return true;
	}

	public static Token OnlyOrDefault(this IEnumerable<Token> value)
	{
		Token token = null;
		foreach (Token item in value)
		{
			if (token == null)
			{
				token = item;
				continue;
			}
			token = null;
			break;
		}
		return token;
	}

	public static bool Is(this IEnumerable<Token> value, string expected)
	{
		return value.ToIdentifier()?.Isi(expected) ?? false;
	}

	public static string ToUri(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.Url)
		{
			return token.Data;
		}
		return null;
	}

	public static Length? ToDistance(this IEnumerable<Token> value)
	{
		Token[] value2 = (value as Token[]) ?? value.ToArray();
		Percent? percent = value2.ToPercent();
		if (!percent.HasValue)
		{
			return value2.ToLength();
		}
		return new Length(percent.Value.Value, Length.Unit.Percent);
	}

	public static Length ToLength(this FontSize fontSize)
	{
		return fontSize switch
		{
			FontSize.Big => new Length(1.5f, Length.Unit.Em), 
			FontSize.Huge => new Length(2f, Length.Unit.Em), 
			FontSize.Large => new Length(1.2f, Length.Unit.Em), 
			FontSize.Larger => new Length(120f, Length.Unit.Percent), 
			FontSize.Little => new Length(0.75f, Length.Unit.Em), 
			FontSize.Small => new Length(8f / 9f, Length.Unit.Em), 
			FontSize.Smaller => new Length(80f, Length.Unit.Percent), 
			FontSize.Tiny => new Length(0.6f, Length.Unit.Em), 
			_ => new Length(1f, Length.Unit.Em), 
		};
	}

	public static Percent? ToPercent(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.Percentage)
		{
			return new Percent(((UnitToken)token).Value);
		}
		return null;
	}

	public static Percent? ToPercentOrFraction(this IEnumerable<Token> value)
	{
		Percent? result = ((value as Token[]) ?? value.ToArray()).ToPercent();
		if (result.HasValue)
		{
			return result;
		}
		if (!(value.OnlyOrDefault() is NumberToken numberToken))
		{
			return null;
		}
		try
		{
			return new Percent(numberToken.Value * 100f);
		}
		catch
		{
			return null;
		}
	}

	public static Number? ToPercentOrNumber(this IEnumerable<Token> value)
	{
		Percent? percent = ((value as Token[]) ?? value.ToArray()).ToPercent();
		if (percent.HasValue)
		{
			return new Number(percent.Value.Value, Number.Unit.Percent);
		}
		if (!(value.OnlyOrDefault() is NumberToken numberToken))
		{
			return null;
		}
		try
		{
			return new Number(numberToken.Value, Number.Unit.Float);
		}
		catch
		{
			return null;
		}
	}

	public static string ToCssString(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.String)
		{
			return token.Data;
		}
		return null;
	}

	public static string ToLiterals(this IEnumerable<Token> value)
	{
		List<string> list = new List<string>();
		IEnumerator<Token> enumerator = value.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				Token current = enumerator.Current;
				if (current == null || current.Type != TokenType.Ident)
				{
					return null;
				}
				list.Add(enumerator.Current.Data);
				if (enumerator.MoveNext())
				{
					Token current2 = enumerator.Current;
					if (current2 == null || current2.Type != TokenType.Whitespace)
					{
						return null;
					}
				}
			}
			while (enumerator.MoveNext());
			enumerator.Dispose();
			return string.Join(" ", list);
		}
		enumerator.Dispose();
		return null;
	}

	public static string ToIdentifier(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.Ident)
		{
			return token.Data.ToLowerInvariant();
		}
		return null;
	}

	public static string ToIdentifierCaseInsensitive(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.Ident)
		{
			return token.Data;
		}
		return null;
	}

	public static string ToAnimatableIdentifier(this IEnumerable<Token> value)
	{
		string text = value.ToIdentifier();
		if (text != null && (text.Isi(Keywords.All) || PropertyFactory.Instance.IsAnimatable(text)))
		{
			return text;
		}
		return null;
	}

	public static float? ToSingle(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.Number)
		{
			return ((NumberToken)token).Value;
		}
		return null;
	}

	public static float? ToNaturalSingle(this IEnumerable<Token> value)
	{
		float? num = value.ToSingle();
		if (!(num >= 0f))
		{
			return null;
		}
		return num;
	}

	public static float? ToGreaterOrEqualOneSingle(this IEnumerable<Token> value)
	{
		float? num = value.ToSingle();
		if (!(num >= 1f))
		{
			return null;
		}
		return num;
	}

	public static int? ToInteger(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.Number && ((NumberToken)token).IsInteger)
		{
			return ((NumberToken)token).IntegerValue;
		}
		return null;
	}

	public static int? ToNaturalInteger(this IEnumerable<Token> value)
	{
		int? num = value.ToInteger();
		if (!(num >= 0))
		{
			return null;
		}
		return num;
	}

	public static int? ToPositiveInteger(this IEnumerable<Token> value)
	{
		int? num = value.ToInteger();
		if (!(num > 0))
		{
			return null;
		}
		return num;
	}

	public static int? ToWeightInteger(this IEnumerable<Token> value)
	{
		int? result = value.ToPositiveInteger();
		if (!result.HasValue || !IsWeight(result.Value))
		{
			return null;
		}
		return result;
	}

	public static int? ToBinary(this IEnumerable<Token> value)
	{
		int? result = value.ToInteger();
		if (!result.HasValue || (result.Value != 0 && result.Value != 1))
		{
			return null;
		}
		return result;
	}

	public static float? ToAlphaValue(this IEnumerable<Token> value)
	{
		Token[] value2 = (value as Token[]) ?? value.ToArray();
		float? num = value2.ToNaturalSingle();
		if (num.HasValue)
		{
			return Math.Min(num.Value, 1f);
		}
		return value2.ToPercent()?.NormalizedValue;
	}

	public static byte? ToRgbComponent(this IEnumerable<Token> value)
	{
		Token[] value2 = (value as Token[]) ?? value.ToArray();
		int? num = value2.ToNaturalInteger();
		if (num.HasValue)
		{
			return (byte)Math.Min(num.Value, 255);
		}
		Percent? percent = value2.ToPercent();
		if (!percent.HasValue)
		{
			return null;
		}
		return (byte)(255f * percent.Value.NormalizedValue);
	}

	public static Angle? ToAngle(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token == null || token.Type != TokenType.Dimension)
		{
			return null;
		}
		UnitToken unitToken = (UnitToken)token;
		Angle.Unit unit = Angle.GetUnit(unitToken.Unit);
		if (unit != Angle.Unit.None)
		{
			return new Angle(unitToken.Value, unit);
		}
		return null;
	}

	public static Angle? ToAngleNumber(this IEnumerable<Token> value)
	{
		Token[] value2 = (value as Token[]) ?? value.ToArray();
		Angle? angle = value2.ToAngle();
		if (angle.HasValue)
		{
			return angle.Value;
		}
		float? num = value2.ToSingle();
		if (!num.HasValue)
		{
			return null;
		}
		return new Angle(num.Value, Angle.Unit.Deg);
	}

	public static Length? ToLength(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null)
		{
			switch (token.Type)
			{
			case TokenType.Dimension:
			{
				UnitToken unitToken = (UnitToken)token;
				Length.Unit unit = Length.GetUnit(unitToken.Unit);
				if (unit != Length.Unit.None)
				{
					return new Length(unitToken.Value, unit);
				}
				break;
			}
			case TokenType.Number:
				if (((NumberToken)token).Value == 0f)
				{
					return Length.Zero;
				}
				break;
			}
		}
		return null;
	}

	public static Resolution? ToResolution(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token == null || token.Type != TokenType.Dimension)
		{
			return null;
		}
		UnitToken unitToken = (UnitToken)token;
		Resolution.Unit unit = Resolution.GetUnit(unitToken.Unit);
		if (unit != Resolution.Unit.None)
		{
			return new Resolution(unitToken.Value, unit);
		}
		return null;
	}

	public static Time? ToTime(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token == null || token.Type != TokenType.Dimension)
		{
			return null;
		}
		UnitToken unitToken = (UnitToken)token;
		Time.Unit unit = Time.GetUnit(unitToken.Unit);
		if (unit != Time.Unit.None)
		{
			return new Time(unitToken.Value, unit);
		}
		return null;
	}

	public static Length? ToBorderWidth(this IEnumerable<Token> value)
	{
		Token[] value2 = (value as Token[]) ?? value.ToArray();
		Length? result = value2.ToLength();
		if (result.HasValue)
		{
			return result;
		}
		if (value2.Is(Keywords.Thin))
		{
			return Length.Thin;
		}
		if (value2.Is(Keywords.Medium))
		{
			return Length.Medium;
		}
		if (!value2.Is(Keywords.Thick))
		{
			return result;
		}
		return Length.Thick;
	}

	public static List<List<Token>> ToItems(this IEnumerable<Token> value)
	{
		List<List<Token>> list = new List<List<Token>>();
		List<Token> list2 = new List<Token>();
		int num = 0;
		list.Add(list2);
		foreach (Token item in value)
		{
			bool flag = item.Type == TokenType.Whitespace;
			bool flag2 = item.Type == TokenType.String || item.Type == TokenType.Url || item.Type == TokenType.Function;
			if (num == 0 && (flag | flag2))
			{
				if (list2.Count != 0)
				{
					list2 = new List<Token>();
					list.Add(list2);
				}
				if (flag)
				{
					continue;
				}
			}
			else if (item.Type == TokenType.RoundBracketOpen)
			{
				num++;
			}
			else if (item.Type == TokenType.RoundBracketClose)
			{
				num--;
			}
			list2.Add(item);
		}
		return list;
	}

	public static void Trim(this List<Token> value)
	{
		int num = 0;
		int num2 = value.Count - 1;
		while (num < num2)
		{
			if (value[num].Type == TokenType.Whitespace)
			{
				num++;
				continue;
			}
			if (value[num2].Type != TokenType.Whitespace)
			{
				break;
			}
			num2--;
		}
		value.RemoveRange(++num2, value.Count - num2);
		value.RemoveRange(0, num);
	}

	public static List<List<Token>> ToList(this IEnumerable<Token> value)
	{
		List<List<Token>> list = new List<List<Token>>();
		List<Token> list2 = new List<Token>();
		int num = 0;
		list.Add(list2);
		foreach (Token item in value)
		{
			if (num == 0 && item.Type == TokenType.Comma)
			{
				list2 = new List<Token>();
				list.Add(list2);
				continue;
			}
			switch (item.Type)
			{
			case TokenType.RoundBracketOpen:
				num++;
				break;
			case TokenType.RoundBracketClose:
				num--;
				break;
			case TokenType.Whitespace:
				if (list2.Count == 0)
				{
					continue;
				}
				break;
			}
			list2.Add(item);
		}
		foreach (List<Token> item2 in list)
		{
			item2.Trim();
		}
		return list;
	}

	public static string ToText(this IEnumerable<Token> value)
	{
		return string.Join(string.Empty, value.Select((Token m) => m.ToValue()));
	}

	public static Color? ToColor(this IEnumerable<Token> value)
	{
		Token token = value.OnlyOrDefault();
		if (token != null && token.Type == TokenType.Ident)
		{
			return Color.FromName(token.Data);
		}
		if (token != null && token.Type == TokenType.Color && !((ColorToken)token).IsValid)
		{
			return Color.FromHex(token.Data);
		}
		return null;
	}
}

using System;
using Svg.Helpers;

namespace Svg;

public static class CoordinateParser
{
	private static bool MarkState(bool hasMode, ref CoordinateParserState state)
	{
		state.HasMore = hasMode;
		state.CharsPosition++;
		return hasMode;
	}

	public static bool TryGetBool(out bool result, ReadOnlySpan<char> chars, ref CoordinateParserState state)
	{
		int length = chars.Length;
		while (state.CharsPosition < length && state.HasMore)
		{
			if (state.CurrNumState == NumState.Separator)
			{
				char c = chars[state.CharsPosition];
				if (IsCoordSeparator(c))
				{
					state.NewNumState = NumState.Separator;
					state.CharsPosition++;
					continue;
				}
				switch (c)
				{
				case '0':
					result = false;
					state.NewNumState = NumState.Separator;
					state.Position = state.CharsPosition + 1;
					return MarkState(hasMode: true, ref state);
				case '1':
					result = true;
					state.NewNumState = NumState.Separator;
					state.Position = state.CharsPosition + 1;
					return MarkState(hasMode: true, ref state);
				default:
					result = false;
					return MarkState(hasMode: false, ref state);
				}
			}
			result = false;
			return MarkState(hasMode: false, ref state);
		}
		result = false;
		return MarkState(hasMode: false, ref state);
	}

	public static bool TryGetFloat(out float result, ReadOnlySpan<char> chars, ref CoordinateParserState state)
	{
		int length = chars.Length;
		while (state.CharsPosition < length && state.HasMore)
		{
			char c = chars[state.CharsPosition];
			switch (state.CurrNumState)
			{
			case NumState.Separator:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.Integer;
					break;
				}
				if (IsCoordSeparator(c))
				{
					state.NewNumState = NumState.Separator;
					break;
				}
				switch (c)
				{
				case '.':
					state.NewNumState = NumState.DecPlace;
					break;
				case '+':
				case '-':
					state.NewNumState = NumState.Prefix;
					break;
				default:
					state.NewNumState = NumState.Invalid;
					break;
				}
				break;
			case NumState.Prefix:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.Integer;
				}
				else if (c == '.')
				{
					state.NewNumState = NumState.DecPlace;
				}
				else
				{
					state.NewNumState = NumState.Invalid;
				}
				break;
			case NumState.Integer:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.Integer;
					break;
				}
				if (IsCoordSeparator(c))
				{
					state.NewNumState = NumState.Separator;
					break;
				}
				switch (c)
				{
				case '.':
					state.NewNumState = NumState.DecPlace;
					break;
				case 'E':
				case 'e':
					state.NewNumState = NumState.Exponent;
					break;
				case '+':
				case '-':
					state.NewNumState = NumState.Prefix;
					break;
				default:
					state.NewNumState = NumState.Invalid;
					break;
				}
				break;
			case NumState.DecPlace:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.Fraction;
					break;
				}
				if (IsCoordSeparator(c))
				{
					state.NewNumState = NumState.Separator;
					break;
				}
				switch (c)
				{
				case 'E':
				case 'e':
					state.NewNumState = NumState.Exponent;
					break;
				case '+':
				case '-':
					state.NewNumState = NumState.Prefix;
					break;
				default:
					state.NewNumState = NumState.Invalid;
					break;
				}
				break;
			case NumState.Fraction:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.Fraction;
					break;
				}
				if (IsCoordSeparator(c))
				{
					state.NewNumState = NumState.Separator;
					break;
				}
				switch (c)
				{
				case '.':
					state.NewNumState = NumState.DecPlace;
					break;
				case 'E':
				case 'e':
					state.NewNumState = NumState.Exponent;
					break;
				case '+':
				case '-':
					state.NewNumState = NumState.Prefix;
					break;
				default:
					state.NewNumState = NumState.Invalid;
					break;
				}
				break;
			case NumState.Exponent:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.ExpValue;
				}
				else if (IsCoordSeparator(c))
				{
					state.NewNumState = NumState.Invalid;
				}
				else if (c == '+' || c == '-')
				{
					state.NewNumState = NumState.ExpPrefix;
				}
				else
				{
					state.NewNumState = NumState.Invalid;
				}
				break;
			case NumState.ExpPrefix:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.ExpValue;
				}
				else
				{
					state.NewNumState = NumState.Invalid;
				}
				break;
			case NumState.ExpValue:
				if (char.IsNumber(c))
				{
					state.NewNumState = NumState.ExpValue;
					break;
				}
				if (IsCoordSeparator(c))
				{
					state.NewNumState = NumState.Separator;
					break;
				}
				switch (c)
				{
				case '.':
					state.NewNumState = NumState.DecPlace;
					break;
				case '+':
				case '-':
					state.NewNumState = NumState.Prefix;
					break;
				default:
					state.NewNumState = NumState.Invalid;
					break;
				}
				break;
			}
			if (state.CurrNumState != NumState.Separator && state.NewNumState < state.CurrNumState)
			{
				ReadOnlySpan<char> value = chars.Slice(state.Position, state.CharsPosition - state.Position);
				result = StringParser.ToFloat(value);
				state.Position = state.CharsPosition;
				state.CurrNumState = state.NewNumState;
				return MarkState(hasMode: true, ref state);
			}
			if (state.NewNumState != state.CurrNumState && state.CurrNumState == NumState.Separator)
			{
				state.Position = state.CharsPosition;
			}
			if (state.NewNumState == NumState.Invalid)
			{
				result = float.MinValue;
				return MarkState(hasMode: false, ref state);
			}
			state.CurrNumState = state.NewNumState;
			state.CharsPosition++;
		}
		if (state.CurrNumState == NumState.Separator || !state.HasMore || state.Position >= length)
		{
			result = float.MinValue;
			return MarkState(hasMode: false, ref state);
		}
		ReadOnlySpan<char> value2 = chars.Slice(state.Position, length - state.Position);
		result = StringParser.ToFloat(value2);
		state.Position = length;
		return MarkState(hasMode: true, ref state);
	}

	private static bool IsCoordSeparator(char value)
	{
		switch (value)
		{
		case '\t':
		case '\n':
		case '\r':
		case ' ':
		case ',':
			return true;
		default:
			return false;
		}
	}
}

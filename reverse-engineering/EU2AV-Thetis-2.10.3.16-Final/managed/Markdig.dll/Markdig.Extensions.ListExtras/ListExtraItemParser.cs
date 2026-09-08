using System;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Extensions.ListExtras;

public class ListExtraItemParser : OrderedListItemParser
{
	public ListExtraItemParser()
	{
		base.OpeningCharacters = new char[52];
		int num = 0;
		for (char c = 'A'; c <= 'Z'; c = (char)(c + 1))
		{
			base.OpeningCharacters[num++] = c;
			base.OpeningCharacters[num++] = (char)(c - 65 + 97);
		}
	}

	public override bool TryParse(BlockProcessor state, char pendingBulletType, out ListInfo result)
	{
		result = default(ListInfo);
		char currentChar = state.CurrentChar;
		bool flag = CharHelper.IsRomanLetterLowerPartial(currentChar);
		bool flag2 = !flag && CharHelper.IsRomanLetterUpperPartial(currentChar);
		if ((flag | flag2) && (pendingBulletType == '\0' || pendingBulletType == 'i' || pendingBulletType == 'I'))
		{
			int start = state.Start;
			do
			{
				currentChar = state.NextChar();
			}
			while (flag ? CharHelper.IsRomanLetterLowerPartial(currentChar) : CharHelper.IsRomanLetterUpperPartial(currentChar));
			int number = CharHelper.RomanToArabic(state.Line.Text.AsSpan(start, state.Start - start));
			result.OrderedStart = CharHelper.SmallNumberToString(number);
			result.BulletType = (flag ? 'i' : 'I');
			result.DefaultOrderedStart = (flag ? "i" : "I");
		}
		else
		{
			bool flag3 = currentChar.IsAlphaUpper();
			result.OrderedStart = CharHelper.SmallNumberToString((currentChar | 0x20) - 97 + 1);
			result.BulletType = (flag3 ? 'A' : 'a');
			result.DefaultOrderedStart = (flag3 ? "A" : "a");
			state.NextChar();
		}
		if (!TryParseDelimiter(state, out var orderedDelimiter))
		{
			return false;
		}
		result.OrderedDelimiter = orderedDelimiter;
		return true;
	}
}

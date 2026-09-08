using Markdig.Helpers;

namespace Markdig.Parsers;

public class NumberedListItemParser : OrderedListItemParser
{
	public NumberedListItemParser()
	{
		base.OpeningCharacters = new char[10];
		for (int i = 0; i < 10; i++)
		{
			base.OpeningCharacters[i] = (char)(48 + i);
		}
	}

	public override bool TryParse(BlockProcessor state, char pendingBulletType, out ListInfo result)
	{
		result = default(ListInfo);
		char c = state.CurrentChar;
		int start = state.Start;
		int num = 0;
		int num2 = -1;
		int num3 = 0;
		while (c.IsDigit())
		{
			num3 = state.Start;
			if (num2 < 0 && c != '0')
			{
				num2 = num3;
			}
			c = state.NextChar();
			num++;
		}
		StringSlice sourceBullet = new StringSlice(state.Line.Text, start, state.Start - 1);
		if (num2 < 0)
		{
			num2 = num3;
		}
		if (num > 9 || !TryParseDelimiter(state, out var orderedDelimiter))
		{
			return false;
		}
		if (num2 == num3)
		{
			result.OrderedStart = CharHelper.SmallNumberToString(state.Line.Text[num2] - 48);
		}
		else
		{
			result.OrderedStart = state.Line.Text.Substring(num2, num3 - num2 + 1);
		}
		result.OrderedDelimiter = orderedDelimiter;
		result.BulletType = '1';
		result.DefaultOrderedStart = "1";
		result.SourceBullet = sourceBullet;
		return true;
	}
}

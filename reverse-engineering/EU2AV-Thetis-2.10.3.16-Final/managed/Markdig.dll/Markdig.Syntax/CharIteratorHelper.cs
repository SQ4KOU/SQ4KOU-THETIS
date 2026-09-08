using Markdig.Helpers;

namespace Markdig.Syntax;

public static class CharIteratorHelper
{
	public static bool TrimStartAndCountNewLines<T>(ref T iterator, out int countNewLines) where T : ICharIterator
	{
		NewLine lastLine;
		return TrimStartAndCountNewLines(ref iterator, out countNewLines, out lastLine);
	}

	public static bool TrimStartAndCountNewLines<T>(ref T iterator, out int countNewLines, out NewLine lastLine) where T : ICharIterator
	{
		countNewLines = 0;
		char c = iterator.CurrentChar;
		bool result = false;
		lastLine = NewLine.None;
		while (c.IsWhitespace())
		{
			if (c == '\n' || c == '\r')
			{
				if (c == '\r' && iterator.PeekChar() == '\n')
				{
					lastLine = NewLine.CarriageReturnLineFeed;
					iterator.SkipChar();
				}
				else
				{
					switch (c)
					{
					case '\n':
						lastLine = NewLine.LineFeed;
						break;
					case '\r':
						lastLine = NewLine.CarriageReturn;
						break;
					}
				}
				countNewLines++;
			}
			else
			{
				lastLine = NewLine.None;
			}
			result = true;
			c = iterator.NextChar();
		}
		return result;
	}
}

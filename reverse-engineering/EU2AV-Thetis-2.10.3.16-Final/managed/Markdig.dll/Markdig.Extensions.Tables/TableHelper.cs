using Markdig.Helpers;

namespace Markdig.Extensions.Tables;

public static class TableHelper
{
	public static bool ParseColumnHeader(ref StringSlice slice, char delimiterChar, out TableColumnAlign? align, out int delimiterCount)
	{
		return ParseColumnHeaderDetect(ref slice, ref delimiterChar, out align, out delimiterCount);
	}

	public static bool ParseColumnHeaderAuto(ref StringSlice slice, out char delimiterChar, out TableColumnAlign? align)
	{
		delimiterChar = '\0';
		int delimiterCount;
		return ParseColumnHeaderDetect(ref slice, ref delimiterChar, out align, out delimiterCount);
	}

	public static bool ParseColumnHeaderDetect(ref StringSlice slice, ref char delimiterChar, out TableColumnAlign? align, out int delimiterCount)
	{
		align = null;
		delimiterCount = 0;
		slice.TrimStart();
		char currentChar = slice.CurrentChar;
		bool flag = false;
		bool flag2 = false;
		if (currentChar == ':')
		{
			flag = true;
			slice.SkipChar();
		}
		slice.TrimStart();
		currentChar = slice.CurrentChar;
		if (delimiterChar == '\0')
		{
			if (currentChar != '=' && currentChar != '-')
			{
				return false;
			}
			delimiterChar = currentChar;
		}
		delimiterCount = slice.CountAndSkipChar(delimiterChar);
		if (delimiterCount == 0)
		{
			return false;
		}
		slice.TrimStart();
		currentChar = slice.CurrentChar;
		if (currentChar == ':')
		{
			flag2 = true;
			slice.SkipChar();
		}
		slice.TrimStart();
		align = ((flag & flag2) ? new TableColumnAlign?(TableColumnAlign.Center) : (flag2 ? new TableColumnAlign?(TableColumnAlign.Right) : (flag ? new TableColumnAlign?(TableColumnAlign.Left) : ((TableColumnAlign?)null))));
		return true;
	}
}

using System.Collections.Generic;
using System.Globalization;

namespace ExCSS;

internal sealed class RangeToken : Token
{
	public string Start { get; }

	public string End { get; }

	public string[] SelectedRange { get; }

	private string[] GetRange()
	{
		int i = int.Parse(Start, NumberStyles.HexNumber);
		if (i > 1114111)
		{
			return null;
		}
		if (End == null)
		{
			return new string[1] { i.ConvertFromUtf32() };
		}
		List<string> list = new List<string>();
		int num = int.Parse(End, NumberStyles.HexNumber);
		if (num > 1114111)
		{
			num = 1114111;
		}
		for (; i <= num; i++)
		{
			list.Add(i.ConvertFromUtf32());
		}
		return list.ToArray();
	}

	public RangeToken(string range, TextPosition position)
		: base(TokenType.Range, range, position)
	{
		Start = range.Replace('?', '0');
		End = range.Replace('?', 'F');
		SelectedRange = GetRange();
	}

	public RangeToken(string start, string end, TextPosition position)
		: base(TokenType.Range, start + "-" + end, position)
	{
		Start = start;
		End = end;
		SelectedRange = GetRange();
	}
}

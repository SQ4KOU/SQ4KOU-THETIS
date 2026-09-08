using System;

namespace Markdig.Helpers;

public struct LineReader
{
	private readonly string _text;

	public int SourcePosition { get; private set; }

	public LineReader(string text)
	{
		if (text == null)
		{
			ThrowHelper.ArgumentNullException_text();
		}
		_text = text;
		SourcePosition = 0;
	}

	public StringSlice ReadLine()
	{
		string text = _text;
		int num = text.Length;
		int sourcePosition = SourcePosition;
		int num2 = int.MaxValue;
		NewLine newLine = NewLine.None;
		if ((uint)sourcePosition >= (uint)num)
		{
			text = null;
		}
		else
		{
			int num3 = text.AsSpan(sourcePosition).IndexOfAny('\r', '\n');
			if (num3 >= 0)
			{
				num = sourcePosition + num3;
				num2 = num + 1;
				if ((uint)num < (uint)text.Length && text[num] == '\r')
				{
					if ((uint)num2 < (uint)text.Length && text[num2] == '\n')
					{
						newLine = NewLine.CarriageReturnLineFeed;
						num2++;
					}
					else
					{
						newLine = NewLine.CarriageReturn;
					}
				}
				else
				{
					newLine = NewLine.LineFeed;
				}
			}
		}
		SourcePosition = num2;
		return new StringSlice(text, sourcePosition, num - 1, newLine, dummy: false);
	}
}

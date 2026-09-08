using System;

namespace ExCSS;

public class StylesheetText
{
	private readonly TextSource _source;

	public TextRange Range { get; }

	public string Text
	{
		get
		{
			int num = Math.Max(Range.Start.Position - 1, 0);
			int num2 = Range.End.Position + 1 - Range.Start.Position;
			string text = _source.Text;
			if (num + num2 > text.Length)
			{
				num2 = text.Length - num;
			}
			return text.Substring(num, num2);
		}
	}

	internal StylesheetText(TextRange range, TextSource source)
	{
		Range = range;
		_source = source;
	}
}

using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Markdig.Extensions.Mathematics;

public class MathInlineParser : InlineParser
{
	public string DefaultClass { get; set; }

	public MathInlineParser()
	{
		base.OpeningCharacters = new char[1] { '$' };
		DefaultClass = "math";
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		char currentChar = slice.CurrentChar;
		char c = slice.PeekCharExtra(-1);
		if (c == currentChar)
		{
			return false;
		}
		int start = slice.Start;
		int num = 1;
		char c2 = slice.NextChar();
		if (c2 == currentChar)
		{
			num++;
			c2 = slice.NextChar();
		}
		c.CheckUnicodeCategory(out var space, out var punctuation);
		c2.CheckUnicodeCategory(out var space2, out var punctuation2);
		if (!space && !punctuation)
		{
			return false;
		}
		bool result = false;
		int num2 = 0;
		while (c2.IsSpaceOrTab())
		{
			c2 = slice.NextChar();
		}
		int start2 = slice.Start;
		int num3 = 0;
		c = currentChar;
		int num4 = -1;
		while (true)
		{
			switch (c2)
			{
			case '\n':
			case '\r':
				return false;
			default:
				if (c != '\\')
				{
					if (c2.IsSpaceOrTab())
					{
						if (num4 < 0)
						{
							num4 = slice.Start;
						}
					}
					else
					{
						bool flag = c2 == currentChar;
						if (flag)
						{
							num2 += slice.CountAndSkipChar(currentChar);
							c2 = slice.CurrentChar;
						}
						if (num2 >= num)
						{
							break;
						}
						num4 = -1;
						if (flag)
						{
							c = currentChar;
							continue;
						}
					}
				}
				if (num2 > 0)
				{
					num2 = 0;
					continue;
				}
				c = c2;
				c2 = slice.NextChar();
				continue;
			case '\0':
				break;
			}
			break;
		}
		if (num2 >= num)
		{
			c.CheckUnicodeCategory(out var space3, out punctuation2);
			c2.CheckUnicodeCategory(out var space4, out var punctuation3);
			if ((!punctuation3 && !space4) || space2 != space3)
			{
				return false;
			}
			num3 = ((!space3 || num4 <= 0) ? (slice.Start - 1) : (num4 + num - 1));
			MathInline mathInline = new MathInline
			{
				Span = new SourceSpan(processor.GetSourcePosition(start, out var lineIndex, out var column), processor.GetSourcePosition(slice.Start - 1)),
				Line = lineIndex,
				Column = column,
				Delimiter = currentChar,
				DelimiterCount = num,
				Content = slice
			};
			mathInline.Content.Start = start2;
			mathInline.Content.End = num3 - num;
			if (DefaultClass != null)
			{
				mathInline.GetAttributes().AddClass(DefaultClass);
			}
			processor.Inline = mathInline;
			result = true;
		}
		return result;
	}
}

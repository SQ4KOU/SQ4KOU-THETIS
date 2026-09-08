using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public class EscapeInlineParser : InlineParser
{
	public EscapeInlineParser()
	{
		base.OpeningCharacters = new char[1] { '\\' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		int start = slice.Start;
		char c = slice.NextChar();
		int lineIndex;
		int column;
		if (c.IsAsciiPunctuation())
		{
			processor.Inline = new LiteralInline
			{
				Content = new StringSlice(slice.Text, slice.Start, slice.Start),
				Span = 
				{
					Start = processor.GetSourcePosition(start, out lineIndex, out column)
				},
				Line = lineIndex,
				Column = column,
				IsFirstCharacterEscaped = true
			};
			processor.Inline.Span.End = processor.Inline.Span.Start + 1;
			slice.SkipChar();
			return true;
		}
		if (c == '\n' || c == '\r')
		{
			NewLine newLine = ((c == '\n') ? NewLine.LineFeed : NewLine.CarriageReturn);
			if (c == '\r' && slice.PeekChar() == '\n')
			{
				newLine = NewLine.CarriageReturnLineFeed;
			}
			LineBreakInline lineBreakInline = (LineBreakInline)(processor.Inline = new LineBreakInline
			{
				IsHard = true,
				IsBackslash = true,
				Span = 
				{
					Start = processor.GetSourcePosition(start, out lineIndex, out column)
				},
				Line = lineIndex,
				Column = column
			});
			if (processor.TrackTrivia)
			{
				lineBreakInline.NewLine = newLine;
			}
			lineBreakInline.Span.End = lineBreakInline.Span.Start + 1;
			slice.SkipChar();
			if (newLine == NewLine.CarriageReturnLineFeed)
			{
				slice.SkipChar();
			}
			return true;
		}
		return false;
	}
}

using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public class LineBreakInlineParser : InlineParser
{
	public bool EnableSoftAsHard { get; set; }

	public LineBreakInlineParser()
	{
		base.OpeningCharacters = new char[2] { '\n', '\r' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		if (!processor.Block.IsParagraphBlock)
		{
			return false;
		}
		int start = slice.Start;
		bool flag = slice.PeekCharExtra(-1).IsSpace() && slice.PeekCharExtra(-2).IsSpace();
		NewLine newLine = NewLine.LineFeed;
		if (processor.TrackTrivia)
		{
			if (slice.CurrentChar == '\r')
			{
				if (slice.PeekChar() == '\n')
				{
					newLine = NewLine.CarriageReturnLineFeed;
					slice.SkipChar();
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
		else if (slice.CurrentChar == '\r' && slice.PeekChar() == '\n')
		{
			slice.SkipChar();
		}
		slice.SkipChar();
		processor.Inline = new LineBreakInline
		{
			Span = 
			{
				Start = processor.GetSourcePosition(start, out var lineIndex, out var column)
			},
			IsHard = (EnableSoftAsHard || ((slice.Start != 0) & flag)),
			Line = lineIndex,
			Column = column,
			NewLine = newLine
		};
		processor.Inline.Span.End = processor.Inline.Span.Start + ((newLine == NewLine.CarriageReturnLineFeed) ? 1 : 0);
		return true;
	}
}

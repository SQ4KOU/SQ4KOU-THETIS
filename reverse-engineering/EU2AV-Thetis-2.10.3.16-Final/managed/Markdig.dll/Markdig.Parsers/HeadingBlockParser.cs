using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class HeadingBlockParser : BlockParser, IAttributesParseable
{
	public int MaxLeadingCount { get; set; } = 6;

	public TryParseAttributesDelegate? TryParseAttributes { get; set; }

	public HeadingBlockParser()
	{
		base.OpeningCharacters = new char[1] { '#' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		int column = processor.Column;
		StringSlice line = processor.Line;
		int start = line.Start;
		char c = line.CurrentChar;
		char c2 = c;
		int num = 0;
		while (c != 0 && num <= MaxLeadingCount && c == c2)
		{
			c = processor.NextChar();
			num++;
		}
		if (num > 0 && num <= MaxLeadingCount && (c.IsSpaceOrTab() || c == '\0'))
		{
			StringSlice triviaAfterAtxHeaderChar = StringSlice.Empty;
			if (processor.TrackTrivia && c.IsSpaceOrTab())
			{
				triviaAfterAtxHeaderChar = new StringSlice(processor.Line.Text, processor.Start, processor.Start);
				processor.NextChar();
			}
			HeadingBlock headingBlock = new HeadingBlock(this)
			{
				HeaderChar = c2,
				Level = num,
				Column = column,
				Span = 
				{
					Start = start
				}
			};
			if (processor.TrackTrivia)
			{
				headingBlock.TriviaAfterAtxHeaderChar = triviaAfterAtxHeaderChar;
				headingBlock.TriviaBefore = processor.UseTrivia(start - 1);
				headingBlock.LinesBefore = processor.TakeLinesBefore();
				headingBlock.NewLine = processor.Line.NewLine;
			}
			else
			{
				processor.GoToColumn(column + num + 1);
			}
			processor.NewBlocks.Push(headingBlock);
			TryParseAttributes?.Invoke(processor, ref processor.Line, headingBlock);
			int num2 = 0;
			int num3 = 0;
			int end = processor.Line.End;
			for (int num4 = processor.Line.End; num4 >= processor.Line.Start - 1; num4--)
			{
				c = processor.Line.Text[num4];
				if (num2 == 0)
				{
					if (c.IsSpaceOrTab())
					{
						continue;
					}
					num2 = 1;
				}
				if (num2 != 1)
				{
					continue;
				}
				if (c == c2)
				{
					num3++;
					continue;
				}
				if (num3 > 0 && c.IsSpaceOrTab())
				{
					processor.Line.End = num4 - 1;
				}
				break;
			}
			headingBlock.Span.End = processor.Line.End;
			if (processor.TrackTrivia && (headingBlock.TriviaAfter = new StringSlice(processor.Line.Text, processor.Line.End + 1, end)).Overlaps(headingBlock.TriviaAfterAtxHeaderChar))
			{
				headingBlock.TriviaAfterAtxHeaderChar = StringSlice.Empty;
			}
			return BlockState.Break;
		}
		processor.Line.Start = start;
		processor.Column = column;
		return BlockState.None;
	}

	public override bool Close(BlockProcessor processor, Block block)
	{
		if (!processor.TrackTrivia)
		{
			((HeadingBlock)block).Lines.Trim();
		}
		return true;
	}
}

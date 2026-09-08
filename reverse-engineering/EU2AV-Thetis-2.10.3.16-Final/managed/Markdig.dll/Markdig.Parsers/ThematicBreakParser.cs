using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class ThematicBreakParser : BlockParser
{
	public static readonly ThematicBreakParser Default = new ThematicBreakParser();

	public ThematicBreakParser()
	{
		base.OpeningCharacters = new char[3] { '-', '_', '*' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		int start = processor.Start;
		StringSlice line = processor.Line;
		int num = 0;
		char currentChar = line.CurrentChar;
		bool flag = false;
		bool flag2 = false;
		for (char c = currentChar; c != 0; c = line.NextChar())
		{
			if (c == currentChar)
			{
				if (flag)
				{
					flag2 = true;
				}
				num++;
			}
			else
			{
				if (!c.IsSpaceOrTab())
				{
					return BlockState.None;
				}
				flag = true;
			}
		}
		ParagraphBlock paragraphBlock = processor.CurrentBlock as ParagraphBlock;
		bool flag3 = paragraphBlock != null && currentChar == '-' && !flag2;
		if (flag3)
		{
			ContainerBlock parent = paragraphBlock.Parent;
			bool flag4 = paragraphBlock.Column != processor.Column;
			if (flag4)
			{
				bool flag5 = ((parent is QuoteBlock || parent is ListItemBlock) ? true : false);
				flag4 = flag5;
			}
			if (flag4)
			{
				flag3 = false;
			}
		}
		if ((num < 3) | flag3)
		{
			return BlockState.None;
		}
		ThematicBreakBlock thematicBreakBlock = new ThematicBreakBlock(this)
		{
			Column = processor.Column,
			Span = new SourceSpan(start, line.End),
			ThematicChar = currentChar,
			ThematicCharCount = num,
			Content = new StringSlice(line.Text, processor.TriviaStart, line.End, line.NewLine)
		};
		if (processor.TrackTrivia)
		{
			thematicBreakBlock.LinesBefore = processor.TakeLinesBefore();
			thematicBreakBlock.NewLine = processor.Line.NewLine;
		}
		processor.NewBlocks.Push(thematicBreakBlock);
		return BlockState.BreakDiscard;
	}
}

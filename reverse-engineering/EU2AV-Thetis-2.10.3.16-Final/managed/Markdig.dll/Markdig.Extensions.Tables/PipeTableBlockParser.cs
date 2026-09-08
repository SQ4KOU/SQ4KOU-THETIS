using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Tables;

public class PipeTableBlockParser : BlockParser
{
	public PipeTableBlockParser()
	{
		base.OpeningCharacters = new char[1] { '-' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		ParagraphBlock paragraphBlock = processor.CurrentBlock as ParagraphBlock;
		if (processor.IsCodeIndent || paragraphBlock == null)
		{
			return BlockState.None;
		}
		StringSlice line = processor.Line;
		int num = 0;
		while (true)
		{
			char c = line.NextChar();
			if (c == '\0')
			{
				if (num > 0)
				{
					paragraphBlock.AppendLine(ref processor.Line, processor.Column, processor.LineIndex, processor.Line.Start, processor.TrackTrivia);
					paragraphBlock.IsOpen = true;
					return BlockState.BreakDiscard;
				}
				return BlockState.None;
			}
			if (!c.IsSpace() && c != '-' && c != '|' && c != ':')
			{
				break;
			}
			if (c == '|')
			{
				num++;
			}
		}
		return BlockState.None;
	}
}

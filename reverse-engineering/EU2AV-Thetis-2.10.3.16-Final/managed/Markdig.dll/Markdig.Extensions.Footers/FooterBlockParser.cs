using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Footers;

public class FooterBlockParser : BlockParser
{
	public FooterBlockParser()
	{
		base.OpeningCharacters = new char[1] { '^' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		int column = processor.Column;
		int start = processor.Start;
		char currentChar = processor.CurrentChar;
		if (processor.PeekChar(1) != currentChar)
		{
			return BlockState.None;
		}
		processor.NextChar();
		if (processor.NextChar().IsSpaceOrTab())
		{
			processor.NextColumn();
		}
		processor.NewBlocks.Push(new FooterBlock(this)
		{
			Span = new SourceSpan(start, processor.Line.End),
			OpeningCharacter = currentChar,
			Column = column,
			Line = processor.LineIndex
		});
		return BlockState.Continue;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		FooterBlock footerBlock = (FooterBlock)block;
		char currentChar = processor.CurrentChar;
		BlockState result = BlockState.Continue;
		if (currentChar != footerBlock.OpeningCharacter || processor.PeekChar(1) != currentChar)
		{
			result = (processor.IsBlankLine ? BlockState.BreakDiscard : BlockState.None);
		}
		else
		{
			processor.NextChar();
			currentChar = processor.NextChar();
			if (currentChar.IsSpace())
			{
				processor.NextChar();
			}
			block.UpdateSpanEnd(processor.Line.End);
		}
		return result;
	}
}

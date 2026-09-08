using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class QuoteBlockParser : BlockParser
{
	public QuoteBlockParser()
	{
		base.OpeningCharacters = new char[1] { '>' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		int start = processor.Start;
		char currentChar = processor.CurrentChar;
		int column = processor.Column;
		char c = processor.NextChar();
		QuoteBlock quoteBlock = new QuoteBlock(this)
		{
			QuoteChar = currentChar,
			Column = column,
			Span = new SourceSpan(start, processor.Line.End)
		};
		if (processor.TrackTrivia)
		{
			quoteBlock.LinesBefore = processor.TakeLinesBefore();
		}
		bool hasSpaceAfterQuoteChar = false;
		switch (c)
		{
		case ' ':
			processor.NextColumn();
			hasSpaceAfterQuoteChar = true;
			processor.SkipFirstUnwindSpace = true;
			break;
		case '\t':
			processor.NextColumn();
			break;
		}
		if (processor.TrackTrivia)
		{
			StringSlice triviaBefore = processor.UseTrivia(start - 1);
			StringSlice triviaAfter = StringSlice.Empty;
			bool flag = false;
			if (processor.Line.IsEmptyOrWhitespace())
			{
				processor.TriviaStart = processor.Start;
				triviaAfter = processor.UseTrivia(processor.Line.End);
				flag = true;
			}
			if (!flag)
			{
				processor.TriviaStart = processor.Start;
			}
			quoteBlock.QuoteLines.Add(new QuoteBlockLine
			{
				TriviaBefore = triviaBefore,
				TriviaAfter = triviaAfter,
				QuoteChar = true,
				HasSpaceAfterQuoteChar = hasSpaceAfterQuoteChar,
				NewLine = processor.Line.NewLine
			});
		}
		processor.NewBlocks.Push(quoteBlock);
		return BlockState.Continue;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		QuoteBlock quoteBlock = (QuoteBlock)block;
		int start = processor.Start;
		char currentChar = processor.CurrentChar;
		if (currentChar != quoteBlock.QuoteChar)
		{
			if (processor.IsBlankLine)
			{
				return BlockState.BreakDiscard;
			}
			if (processor.TrackTrivia)
			{
				quoteBlock.QuoteLines.Add(new QuoteBlockLine
				{
					QuoteChar = false,
					NewLine = processor.Line.NewLine
				});
			}
			return BlockState.None;
		}
		bool hasSpaceAfterQuoteChar = false;
		switch (processor.NextChar())
		{
		case ' ':
			processor.NextColumn();
			hasSpaceAfterQuoteChar = true;
			processor.SkipFirstUnwindSpace = true;
			break;
		case '\t':
			processor.NextColumn();
			break;
		}
		if (processor.TrackTrivia)
		{
			StringSlice triviaBefore = processor.UseTrivia(start - 1);
			StringSlice triviaAfter = StringSlice.Empty;
			bool flag = false;
			if (processor.Line.IsEmptyOrWhitespace())
			{
				processor.TriviaStart = processor.Start;
				triviaAfter = processor.UseTrivia(processor.Line.End);
				flag = true;
			}
			quoteBlock.QuoteLines.Add(new QuoteBlockLine
			{
				QuoteChar = true,
				HasSpaceAfterQuoteChar = hasSpaceAfterQuoteChar,
				TriviaBefore = triviaBefore,
				TriviaAfter = triviaAfter,
				NewLine = processor.Line.NewLine
			});
			if (!flag)
			{
				processor.TriviaStart = processor.Start;
			}
		}
		block.UpdateSpanEnd(processor.Line.End);
		return BlockState.Continue;
	}
}

using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class ParagraphBlockParser : BlockParser
{
	public bool ParseSetexHeadings { get; set; } = true;

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsBlankLine)
		{
			return BlockState.None;
		}
		ParagraphBlock paragraphBlock = new ParagraphBlock(this)
		{
			Column = processor.Column,
			Span = new SourceSpan(processor.Line.Start, processor.Line.End)
		};
		if (processor.TrackTrivia)
		{
			paragraphBlock.LinesBefore = processor.TakeLinesBefore();
			paragraphBlock.NewLine = processor.Line.NewLine;
		}
		processor.NewBlocks.Push(paragraphBlock);
		return BlockState.Continue;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		if (processor.IsBlankLine)
		{
			return BlockState.BreakDiscard;
		}
		if (!processor.IsCodeIndent && ParseSetexHeadings)
		{
			return TryParseSetexHeading(processor, block);
		}
		block.NewLine = processor.Line.NewLine;
		block.UpdateSpanEnd(processor.Line.End);
		return BlockState.Continue;
	}

	public override bool Close(BlockProcessor processor, Block block)
	{
		if (block is ParagraphBlock paragraphBlock)
		{
			ref StringLineGroup lines = ref paragraphBlock.Lines;
			if (processor.TrackTrivia)
			{
				TryMatchLinkReferenceDefinitionTrivia(ref lines, processor, paragraphBlock);
			}
			else
			{
				TryMatchLinkReferenceDefinition(ref lines, processor);
			}
			int count = lines.Count;
			if (count == 0)
			{
				return false;
			}
			if (!processor.TrackTrivia)
			{
				for (int i = 0; i < count; i++)
				{
					lines.Lines[i].Slice.TrimStart();
				}
				lines.Lines[count - 1].Slice.TrimEnd();
			}
		}
		return true;
	}

	private BlockState TryParseSetexHeading(BlockProcessor state, Block block)
	{
		StringSlice line = state.Line;
		int start = line.Start;
		int count = 0;
		char headingChar = GetHeadingChar(ref line, ref count);
		if (headingChar != 0)
		{
			ParagraphBlock paragraphBlock = (ParagraphBlock)block;
			bool flag = ((!state.TrackTrivia) ? TryMatchLinkReferenceDefinition(ref paragraphBlock.Lines, state) : TryMatchLinkReferenceDefinitionTrivia(ref paragraphBlock.Lines, state, paragraphBlock));
			ContainerBlock parent = block.Parent;
			bool flag2 = !state.IsLazy || paragraphBlock.Column == state.Column || (!(parent is QuoteBlock) && !(parent is ListItemBlock));
			if ((!flag || paragraphBlock.Lines.Count != 0) & flag2)
			{
				state.Discard(paragraphBlock);
				while (state.CurrentChar == headingChar)
				{
					state.NextChar();
				}
				int level = ((headingChar == '=') ? 1 : 2);
				HeadingBlock headingBlock = new HeadingBlock(this)
				{
					Column = paragraphBlock.Column,
					Span = new SourceSpan(paragraphBlock.Span.Start, line.End),
					Level = level,
					Lines = paragraphBlock.Lines,
					IsSetext = true,
					HeaderCharCount = count
				};
				if (state.TrackTrivia)
				{
					headingBlock.LinesBefore = paragraphBlock.LinesBefore;
					headingBlock.TriviaBefore = state.UseTrivia(start - 1);
					headingBlock.TriviaAfter = new StringSlice(state.Line.Text, state.Start, line.End);
					headingBlock.NewLine = state.Line.NewLine;
					headingBlock.SetextNewline = paragraphBlock.NewLine;
				}
				else
				{
					headingBlock.Lines.Trim();
				}
				state.NewBlocks.Push(headingBlock);
				return BlockState.BreakDiscard;
			}
		}
		block.UpdateSpanEnd(state.Line.End);
		return BlockState.Continue;
	}

	private static char GetHeadingChar(ref StringSlice line, ref int count)
	{
		char currentChar = line.CurrentChar;
		if (currentChar == '=' || currentChar == '-')
		{
			count = line.CountAndSkipChar(currentChar);
			while (line.CurrentChar.IsSpaceOrTab())
			{
				line.NextChar();
			}
			if (line.IsEmpty)
			{
				return currentChar;
			}
		}
		return '\0';
	}

	private static bool TryMatchLinkReferenceDefinition(ref StringLineGroup lines, BlockProcessor state)
	{
		bool result = false;
		while (true)
		{
			StringLineGroup.Iterator text = lines.ToCharIterator();
			if (!LinkReferenceDefinition.TryParse(ref text, out LinkReferenceDefinition block))
			{
				break;
			}
			state.Document.SetLinkReferenceDefinition(block.Label, block, addGroup: true);
			result = true;
			block.Line = lines.Lines[0].Line;
			block.Span = lines.ConvertToAbsoluteSpan(block.Span);
			block.LabelSpan = lines.ConvertToAbsoluteSpan(block.LabelSpan);
			block.UrlSpan = lines.ConvertToAbsoluteSpan(block.UrlSpan);
			block.TitleSpan = lines.ConvertToAbsoluteSpan(block.TitleSpan);
			lines = text.Remaining();
		}
		return result;
	}

	private static bool TryMatchLinkReferenceDefinitionTrivia(ref StringLineGroup lines, BlockProcessor state, ParagraphBlock paragraph)
	{
		bool result = false;
		while (true)
		{
			StringLineGroup.Iterator text = lines.ToCharIterator();
			if (!LinkReferenceDefinition.TryParseTrivia(ref text, out LinkReferenceDefinition block, out SourceSpan triviaBeforeLabel, out SourceSpan labelWithTrivia, out SourceSpan triviaBeforeUrl, out SourceSpan unescapedUrl, out SourceSpan triviaBeforeTitle, out SourceSpan unescapedTitle, out SourceSpan triviaAfterTitle))
			{
				break;
			}
			state.Document.SetLinkReferenceDefinition(block.Label, block, addGroup: false);
			block.Parent = null;
			result = true;
			block.Line = lines.Lines[0].Line;
			string text2 = lines.Lines[0].Slice.Text;
			triviaBeforeLabel = lines.ConvertToAbsoluteSpan(triviaBeforeLabel);
			labelWithTrivia = lines.ConvertToAbsoluteSpan(labelWithTrivia);
			triviaBeforeUrl = lines.ConvertToAbsoluteSpan(triviaBeforeUrl);
			unescapedUrl = lines.ConvertToAbsoluteSpan(unescapedUrl);
			triviaBeforeTitle = lines.ConvertToAbsoluteSpan(triviaBeforeTitle);
			unescapedTitle = lines.ConvertToAbsoluteSpan(unescapedTitle);
			triviaAfterTitle = lines.ConvertToAbsoluteSpan(triviaAfterTitle);
			block.Span = lines.ConvertToAbsoluteSpan(block.Span);
			block.TriviaBefore = new StringSlice(text2, triviaBeforeLabel.Start, triviaBeforeLabel.End);
			block.LabelSpan = lines.ConvertToAbsoluteSpan(block.LabelSpan);
			block.LabelWithTrivia = new StringSlice(text2, labelWithTrivia.Start, labelWithTrivia.End);
			block.TriviaBeforeUrl = new StringSlice(text2, triviaBeforeUrl.Start, triviaBeforeUrl.End);
			block.UrlSpan = lines.ConvertToAbsoluteSpan(block.UrlSpan);
			block.UnescapedUrl = new StringSlice(text2, unescapedUrl.Start, unescapedUrl.End);
			block.TriviaBeforeTitle = new StringSlice(text2, triviaBeforeTitle.Start, triviaBeforeTitle.End);
			block.TitleSpan = lines.ConvertToAbsoluteSpan(block.TitleSpan);
			block.UnescapedTitle = new StringSlice(text2, unescapedTitle.Start, unescapedTitle.End);
			block.TriviaAfter = new StringSlice(text2, triviaAfterTitle.Start, triviaAfterTitle.End);
			block.LinesBefore = paragraph.LinesBefore;
			state.LinesBefore = paragraph.LinesAfter;
			lines = text.Remaining();
			int index = paragraph.Parent.IndexOf(paragraph);
			paragraph.Parent.Insert(index, block);
		}
		return result;
	}
}

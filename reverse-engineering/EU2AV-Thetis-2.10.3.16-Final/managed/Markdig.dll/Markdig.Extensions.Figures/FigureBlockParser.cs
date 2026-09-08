using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Figures;

public class FigureBlockParser : BlockParser
{
	public FigureBlockParser()
	{
		base.OpeningCharacters = new char[1] { '^' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		StringSlice slice = processor.Line;
		char currentChar = slice.CurrentChar;
		int num = slice.CountAndSkipChar(currentChar);
		if (num < 3)
		{
			return BlockState.None;
		}
		int start = processor.Start;
		int column = processor.Column;
		Figure figure = new Figure(this)
		{
			Span = new SourceSpan(start, slice.End),
			Line = processor.LineIndex,
			Column = column,
			OpeningCharacter = currentChar,
			OpeningCharacterCount = num
		};
		slice.TrimStart();
		if (!slice.IsEmpty)
		{
			FigureCaption figureCaption = new FigureCaption(this)
			{
				Span = new SourceSpan(slice.Start, slice.End),
				Line = processor.LineIndex,
				Column = column + slice.Start - start,
				IsOpen = false
			};
			figureCaption.AppendLine(ref slice, figureCaption.Column, processor.LineIndex, processor.CurrentLineStartPosition, processor.TrackTrivia);
			figure.Add(figureCaption);
		}
		processor.NewBlocks.Push(figure);
		return BlockState.ContinueDiscard;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		Figure figure = (Figure)block;
		int openingCharacterCount = figure.OpeningCharacterCount;
		char openingCharacter = figure.OpeningCharacter;
		int column = processor.Column;
		StringSlice slice = processor.Line;
		int start = slice.Start;
		openingCharacterCount -= slice.CountAndSkipChar(openingCharacter);
		if (openingCharacterCount <= 0 && !processor.IsCodeIndent)
		{
			slice.TrimStart();
			if (!slice.IsEmpty)
			{
				FigureCaption figureCaption = new FigureCaption(this)
				{
					Span = new SourceSpan(slice.Start, slice.End),
					Line = processor.LineIndex,
					Column = column + slice.Start - start,
					IsOpen = false
				};
				figureCaption.AppendLine(ref slice, figureCaption.Column, processor.LineIndex, processor.CurrentLineStartPosition, processor.TrackTrivia);
				figure.Add(figureCaption);
			}
			figure.UpdateSpanEnd(slice.End);
			return BlockState.BreakDiscard;
		}
		processor.GoToColumn(processor.ColumnBeforeIndent);
		figure.UpdateSpanEnd(slice.End);
		return BlockState.Continue;
	}
}

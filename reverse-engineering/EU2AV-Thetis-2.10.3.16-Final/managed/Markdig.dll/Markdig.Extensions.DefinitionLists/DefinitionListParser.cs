using System;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.DefinitionLists;

public class DefinitionListParser : BlockParser
{
	public DefinitionListParser()
	{
		base.OpeningCharacters = new char[2] { ':', '~' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		ParagraphBlock paragraphBlock = processor.LastBlock as ParagraphBlock;
		if (processor.IsCodeIndent || paragraphBlock == null || paragraphBlock.LastLine - processor.LineIndex > 1)
		{
			return BlockState.None;
		}
		_ = processor.Start;
		int columnBeforeIndent = processor.ColumnBeforeIndent;
		processor.NextChar();
		processor.ParseIndent();
		int num = processor.Column - columnBeforeIndent;
		if (num < 4)
		{
			processor.GoToColumn(columnBeforeIndent);
			return BlockState.None;
		}
		if (num > 4)
		{
			processor.GoToColumn(columnBeforeIndent + 4);
		}
		ContainerBlock parent = paragraphBlock.Parent;
		DefinitionList definitionList = GetCurrentDefinitionList(paragraphBlock, parent);
		processor.Discard(paragraphBlock);
		if (paragraphBlock.Parent != null)
		{
			paragraphBlock.Parent.Remove(paragraphBlock);
		}
		if (definitionList == null)
		{
			definitionList = new DefinitionList(this)
			{
				Span = new SourceSpan(paragraphBlock.Span.Start, processor.Line.End),
				Column = paragraphBlock.Column,
				Line = paragraphBlock.Line
			};
			parent.Add(definitionList);
		}
		DefinitionItem definitionItem = new DefinitionItem(this)
		{
			Line = processor.LineIndex,
			Column = columnBeforeIndent,
			Span = new SourceSpan(paragraphBlock.Span.Start, processor.Line.End),
			OpeningCharacter = processor.CurrentChar
		};
		for (int i = 0; i < paragraphBlock.Lines.Count; i++)
		{
			StringLine stringLine = paragraphBlock.Lines.Lines[i];
			DefinitionTerm definitionTerm = new DefinitionTerm(this)
			{
				Column = paragraphBlock.Column,
				Line = stringLine.Line,
				Span = new SourceSpan(paragraphBlock.Span.Start, paragraphBlock.Span.End),
				IsOpen = false
			};
			definitionTerm.AppendLine(ref stringLine.Slice, stringLine.Column, stringLine.Line, stringLine.Position, processor.TrackTrivia);
			definitionItem.Add(definitionTerm);
		}
		definitionList.Add(definitionItem);
		processor.Open(definitionItem);
		definitionList.UpdateSpanEnd(processor.Line.End);
		return BlockState.Continue;
	}

	private static DefinitionList? GetCurrentDefinitionList(ParagraphBlock paragraphBlock, ContainerBlock previousParent)
	{
		int num = previousParent.IndexOf(paragraphBlock) - 1;
		if (num < 0)
		{
			return null;
		}
		Block block = previousParent[num];
		if (!(block is DefinitionList result))
		{
			if (block is BlankLineBlock && num > 0 && previousParent[num - 1] is DefinitionList result2)
			{
				previousParent.RemoveAt(num);
				return result2;
			}
			return null;
		}
		return result;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		DefinitionItem definitionItem = (DefinitionItem)block;
		if (processor.IsCodeIndent)
		{
			processor.GoToCodeIndent();
			return BlockState.Continue;
		}
		DefinitionList definitionList = (DefinitionList)definitionItem.Parent;
		BlankLineBlock blankLineBlock = definitionItem.LastChild as BlankLineBlock;
		if (Array.IndexOf(base.OpeningCharacters, processor.CurrentChar) >= 0)
		{
			int start = processor.Start;
			int columnBeforeIndent = processor.ColumnBeforeIndent;
			processor.NextChar();
			processor.ParseIndent();
			int num = processor.Column - columnBeforeIndent;
			if (num < 4)
			{
				if (blankLineBlock != null)
				{
					definitionItem.RemoveAt(definitionItem.Count - 1);
				}
				definitionList.Span.End = definitionList.LastChild.Span.End;
				return BlockState.None;
			}
			if (num > 4)
			{
				processor.GoToColumn(columnBeforeIndent + 4);
			}
			processor.Close(definitionItem);
			DefinitionItem definitionItem2 = new DefinitionItem(this)
			{
				Span = new SourceSpan(start, processor.Line.End),
				Line = processor.LineIndex,
				Column = processor.Column,
				OpeningCharacter = processor.CurrentChar
			};
			definitionList.Add(definitionItem2);
			processor.Open(definitionItem2);
			return BlockState.Continue;
		}
		bool flag = definitionItem.LastChild?.IsBreakable ?? true;
		if (processor.IsBlankLine)
		{
			if ((blankLineBlock == null) & flag)
			{
				definitionItem.Add(new BlankLineBlock());
			}
			if (!flag)
			{
				return BlockState.Continue;
			}
			return BlockState.ContinueDiscard;
		}
		ParagraphBlock paragraphBlock = definitionItem.LastChild as ParagraphBlock;
		if (blankLineBlock == null && paragraphBlock != null)
		{
			return BlockState.Continue;
		}
		if (blankLineBlock != null)
		{
			definitionItem.RemoveAt(definitionItem.Count - 1);
		}
		definitionList.Span.End = definitionList.LastChild.Span.End;
		return BlockState.Break;
	}
}

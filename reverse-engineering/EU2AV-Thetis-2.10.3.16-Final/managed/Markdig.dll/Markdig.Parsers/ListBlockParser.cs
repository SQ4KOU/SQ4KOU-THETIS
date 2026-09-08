using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class ListBlockParser : BlockParser
{
	private CharacterMap<ListItemParser>? mapItemParsers;

	public OrderedList<ListItemParser> ItemParsers { get; }

	public ListBlockParser()
	{
		ItemParsers = new OrderedList<ListItemParser>
		{
			new UnorderedListItemParser(),
			new NumberedListItemParser()
		};
	}

	public override void Initialize()
	{
		Dictionary<char, ListItemParser> dictionary = new Dictionary<char, ListItemParser>();
		foreach (ListItemParser itemParser in ItemParsers)
		{
			if (itemParser.OpeningCharacters == null)
			{
				ThrowHelper.InvalidOperationException($"The list item parser of type [{itemParser.GetType()}] cannot have OpeningCharacters to null. It must define a list of valid opening characters");
			}
			char[] openingCharacters = itemParser.OpeningCharacters;
			foreach (char c in openingCharacters)
			{
				if (dictionary.ContainsKey(c))
				{
					ThrowHelper.InvalidOperationException($"A list item parser with the same opening character `{c}` is already registered");
				}
				dictionary.Add(c, itemParser);
			}
		}
		mapItemParsers = new CharacterMap<ListItemParser>(dictionary);
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		ThematicBreakParser thematicBreakParser = ThematicBreakParser.Default;
		if (thematicBreakParser.HasOpeningCharacter(processor.CurrentChar))
		{
			BlockState blockState = thematicBreakParser.TryOpen(processor);
			if (blockState.IsBreak())
			{
				return blockState;
			}
		}
		return TryParseListItem(processor, null);
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		if (block is ListBlock && processor.NextContinue is ListItemBlock)
		{
			return BlockState.Skip;
		}
		ThematicBreakParser thematicBreakParser = ThematicBreakParser.Default;
		BlockState blockState;
		if (!(processor.LastBlock is FencedCodeBlock) && thematicBreakParser.HasOpeningCharacter(processor.CurrentChar))
		{
			blockState = thematicBreakParser.TryOpen(processor);
			if (blockState.IsBreak())
			{
				Block block2 = processor.NewBlocks.Pop();
				if (processor.TrackTrivia)
				{
					processor.LinesBefore = block2.LinesBefore;
				}
				return BlockState.None;
			}
		}
		blockState = BlockState.None;
		if (block is ListItemBlock listItem)
		{
			blockState = TryContinueListItem(processor, listItem);
		}
		if (blockState == BlockState.None)
		{
			blockState = TryParseListItem(processor, block);
		}
		return blockState;
	}

	private BlockState TryContinueListItem(BlockProcessor state, ListItemBlock listItem)
	{
		ListBlock listBlock = (ListBlock)listItem.Parent;
		if (state.IsBlankLine)
		{
			if (state.CurrentBlock != null && state.CurrentBlock.IsBreakable)
			{
				if (!(state.NextContinue is ListBlock))
				{
					listBlock.CountAllBlankLines++;
					if (!state.TrackTrivia)
					{
						listItem.Add(new BlankLineBlock());
					}
				}
				listBlock.CountBlankLinesReset++;
			}
			if (listBlock.CountBlankLinesReset == 1 && listItem.ColumnWidth < 0)
			{
				state.Close(listItem);
				listBlock.CountBlankLinesReset = 0;
				listBlock.IsOpen = true;
				return BlockState.Continue;
			}
			listItem.UpdateSpanEnd(state.Line.End);
			return BlockState.Continue;
		}
		listBlock.CountBlankLinesReset = 0;
		int num = listItem.ColumnWidth;
		if (num < 0)
		{
			num = -num;
		}
		if (state.Indent >= num)
		{
			if (state.Indent > num && state.IsCodeIndent)
			{
				state.GoToColumn(state.ColumnBeforeIndent + num);
			}
			listItem.UpdateSpanEnd(state.Line.End);
			listItem.NewLine = state.Line.NewLine;
			return BlockState.Continue;
		}
		return BlockState.None;
	}

	private BlockState TryParseListItem(BlockProcessor state, Block? block)
	{
		ListItemBlock listItemBlock = block as ListItemBlock;
		ListBlock listBlock = (block as ListBlock) ?? ((ListBlock)(listItemBlock?.Parent));
		if (state.IsCodeIndent && (listItemBlock == null || listItemBlock.LastChild is BlankLineBlock || !listBlock.IsOrdered))
		{
			return BlockState.None;
		}
		int columnBeforeIndent = state.ColumnBeforeIndent;
		int column = state.Column;
		int start = state.Start;
		int end = state.Line.End;
		char currentChar = state.CurrentChar;
		ListItemParser listItemParser = mapItemParsers[currentChar];
		if (listItemParser == null)
		{
			return BlockState.None;
		}
		if (!listItemParser.TryParse(state, listBlock?.BulletType ?? '\0', out var result))
		{
			state.GoToColumn(column);
			return BlockState.None;
		}
		int triviaStart = state.TriviaStart;
		StringSlice triviaBefore = state.UseTrivia(start - 1);
		state.TriviaStart = state.Start;
		bool flag = listItemParser is OrderedListItemParser;
		currentChar = state.CurrentChar;
		int columnWidth;
		if (currentChar == '\0')
		{
			columnWidth = -(state.Column - columnBeforeIndent + 1);
		}
		else
		{
			if (!currentChar.IsSpaceOrTab())
			{
				state.GoToColumn(column);
				state.TriviaStart = triviaStart;
				return BlockState.None;
			}
			state.RestartIndent();
			int column2 = state.Column;
			state.ParseIndent();
			if (state.Indent > 4)
			{
				state.GoToColumn(column2 + 1);
			}
			columnWidth = (state.IsBlankLine ? column2 : state.Column) - columnBeforeIndent;
		}
		if (block == null)
		{
			block = state.LastBlock;
		}
		if (block != null && block.IsParagraphBlock && (state.IsBlankLine || (state.IsOpen(block) && result.BulletType == '1' && !(result.OrderedStart == "1"))))
		{
			state.GoToColumn(column);
			state.TriviaStart = triviaStart;
			return BlockState.None;
		}
		int.TryParse(result.OrderedStart, out var result2);
		ListItemBlock listItemBlock2 = new ListItemBlock(this)
		{
			Column = column,
			ColumnWidth = columnWidth,
			Order = result2,
			Span = new SourceSpan(start, end)
		};
		if (state.TrackTrivia)
		{
			listItemBlock2.TriviaBefore = triviaBefore;
			listItemBlock2.LinesBefore = state.TakeLinesBefore();
			listItemBlock2.NewLine = state.Line.NewLine;
			listItemBlock2.SourceBullet = result.SourceBullet;
		}
		state.NewBlocks.Push(listItemBlock2);
		if (listBlock != null)
		{
			if (listItemBlock != null)
			{
				state.Close(listItemBlock);
			}
			if (listBlock.IsOrdered != flag || listBlock.OrderedDelimiter != result.OrderedDelimiter || listBlock.BulletType != result.BulletType)
			{
				state.Close(listBlock);
				listBlock = null;
			}
		}
		if (listBlock == null)
		{
			ListBlock listBlock2 = new ListBlock(this)
			{
				Column = column,
				Span = new SourceSpan(start, end),
				IsOrdered = flag,
				BulletType = result.BulletType,
				OrderedDelimiter = result.OrderedDelimiter,
				DefaultOrderedStart = result.DefaultOrderedStart,
				OrderedStart = result.OrderedStart
			};
			if (state.TrackTrivia)
			{
				listBlock2.LinesBefore = state.TakeLinesBefore();
			}
			state.NewBlocks.Push(listBlock2);
		}
		return BlockState.Continue;
	}

	public override bool Close(BlockProcessor processor, Block blockToClose)
	{
		if (processor.TrackTrivia)
		{
			return true;
		}
		if (blockToClose is ListBlock { CountAllBlankLines: >0 } listBlock)
		{
			if (listBlock.Parent is ListItemBlock listItemBlock && listBlock.LastChild is ListItemBlock listItemBlock2 && listItemBlock2.LastChild is BlankLineBlock)
			{
				((ListBlock)listItemBlock.Parent).CountAllBlankLines++;
				listItemBlock.Add(new BlankLineBlock());
			}
			int num = listBlock.Count - 1;
			while (num >= 0)
			{
				ListItemBlock listItemBlock3 = (ListItemBlock)listBlock[num];
				for (int num2 = listItemBlock3.Count - 1; num2 >= 0; num2--)
				{
					if (listItemBlock3[num2] is BlankLineBlock)
					{
						if ((num2 == listItemBlock3.Count - 1) ? (num < listBlock.Count - 1) : (num2 > 0))
						{
							listBlock.IsLoose = true;
						}
						listItemBlock3.RemoveAt(num2);
						listBlock.CountAllBlankLines--;
						if (listBlock.CountAllBlankLines == 0)
						{
							goto end_IL_0109;
						}
					}
				}
				num--;
				continue;
				end_IL_0109:
				break;
			}
		}
		return true;
	}
}

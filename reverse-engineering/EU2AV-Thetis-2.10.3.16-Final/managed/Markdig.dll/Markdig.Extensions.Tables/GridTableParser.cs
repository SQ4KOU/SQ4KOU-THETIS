using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Tables;

public class GridTableParser : BlockParser
{
	public GridTableParser()
	{
		base.OpeningCharacters = new char[1] { '+' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		StringSlice slice = processor.Line;
		GridTableState gridTableState = null;
		char currentChar = slice.CurrentChar;
		int start = slice.Start;
		while (currentChar == '+')
		{
			int start2 = slice.Start;
			slice.SkipChar();
			slice.TrimStart();
			currentChar = slice.CurrentChar;
			if (currentChar == '\0')
			{
				break;
			}
			if (!TableHelper.ParseColumnHeader(ref slice, '-', out var align, out var _))
			{
				return BlockState.None;
			}
			if (gridTableState == null)
			{
				gridTableState = new GridTableState(processor.Start, expectRow: true);
			}
			gridTableState.AddColumn(start2 - start, slice.Start - start, align);
			currentChar = slice.CurrentChar;
		}
		if (currentChar != 0 || gridTableState == null)
		{
			return BlockState.None;
		}
		gridTableState.AddLine(ref processor.Line);
		Table table = new Table(this)
		{
			Line = processor.LineIndex,
			Column = processor.Column,
			Span = 
			{
				Start = start
			}
		};
		table.SetData(typeof(GridTableState), gridTableState);
		int num = 0;
		foreach (GridTableState.ColumnSlice columnSlice in gridTableState.ColumnSlices)
		{
			num += columnSlice.End - columnSlice.Start - 1;
		}
		foreach (GridTableState.ColumnSlice columnSlice2 in gridTableState.ColumnSlices)
		{
			TableColumnDefinition item = new TableColumnDefinition
			{
				Width = (float)(columnSlice2.End - columnSlice2.Start - 1) * 100f / (float)num,
				Alignment = columnSlice2.Align
			};
			table.ColumnDefinitions.Add(item);
		}
		processor.NewBlocks.Push(table);
		return BlockState.ContinueDiscard;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		Table table = (Table)block;
		GridTableState gridTableState = (GridTableState)block.GetData(typeof(GridTableState));
		gridTableState.AddLine(ref processor.Line);
		if (processor.CurrentChar == '+')
		{
			table.UpdateSpanEnd(processor.Line.End);
			return HandleNewRow(processor, gridTableState, table);
		}
		if (processor.CurrentChar == '|')
		{
			table.UpdateSpanEnd(processor.Line.End);
			return HandleContents(processor, gridTableState, table);
		}
		TerminateCurrentRow(processor, gridTableState, table, isLastRow: true);
		if (!table.IsValid())
		{
			Undo(processor, gridTableState, table);
		}
		return BlockState.Break;
	}

	private BlockState HandleNewRow(BlockProcessor processor, GridTableState tableState, Table gridTable)
	{
		List<GridTableState.ColumnSlice>? columnSlices = tableState.ColumnSlices;
		SetRowSpanState(columnSlices, processor.Line, out var isHeaderRow, out var hasRowSpan);
		SetColumnSpanState(columnSlices, processor.Line);
		TerminateCurrentRow(processor, tableState, gridTable, isLastRow: false);
		if (isHeaderRow)
		{
			for (int i = 0; i < gridTable.Count; i++)
			{
				((TableRow)gridTable[i]).IsHeader = true;
			}
		}
		tableState.StartRowGroup = gridTable.Count;
		if (hasRowSpan)
		{
			HandleContents(processor, tableState, gridTable);
		}
		return BlockState.ContinueDiscard;
	}

	private static void SetRowSpanState(List<GridTableState.ColumnSlice> columns, StringSlice line, out bool isHeaderRow, out bool hasRowSpan)
	{
		int start = line.Start;
		int end = line.End;
		isHeaderRow = line.PeekChar() == '=' || line.PeekChar(2) == '=';
		hasRowSpan = false;
		foreach (GridTableState.ColumnSlice column in columns)
		{
			if (column.CurrentCell != null)
			{
				line.Start = start + column.Start + 1;
				line.End = Math.Min(start + column.End - 1, end);
				line.Trim();
				if (line.IsEmptyOrWhitespace() || !IsRowSeparator(line))
				{
					hasRowSpan = true;
					column.CurrentCell.RowSpan++;
					column.CurrentCell.AllowClose = false;
				}
				else
				{
					column.CurrentCell.AllowClose = true;
				}
			}
		}
	}

	private static bool IsRowSeparator(StringSlice slice)
	{
		char c = slice.CurrentChar;
		while (c == '-' || c == '=' || c == ':')
		{
			c = slice.NextChar();
		}
		return c == '\0';
	}

	private static void TerminateCurrentRow(BlockProcessor processor, GridTableState tableState, Table gridTable, bool isLastRow)
	{
		List<GridTableState.ColumnSlice> columnSlices = tableState.ColumnSlices;
		TableRow tableRow = null;
		for (int i = 0; i < columnSlices.Count; i++)
		{
			GridTableState.ColumnSlice columnSlice = columnSlices[i];
			if (columnSlice.CurrentCell != null)
			{
				if (tableRow == null)
				{
					TableCell currentCell = columnSlices.First((GridTableState.ColumnSlice c) => c.CurrentCell != null).CurrentCell;
					TableCell currentCell2 = columnSlices.Last((GridTableState.ColumnSlice c) => c.CurrentCell != null).CurrentCell;
					if (tableRow == null)
					{
						tableRow = new TableRow
						{
							Span = new SourceSpan(currentCell.Span.Start, currentCell2.Span.End),
							Line = currentCell.Line
						};
					}
				}
				if (columnSlice.CurrentCell.Parent == null)
				{
					tableRow.Add(columnSlice.CurrentCell);
				}
				if (columnSlice.CurrentCell.AllowClose)
				{
					columnSlice.BlockProcessor.Close(columnSlice.CurrentCell);
				}
			}
			if (columnSlice.BlockProcessor != null && (columnSlice.CurrentCell == null || columnSlice.CurrentCell.AllowClose))
			{
				columnSlice.BlockProcessor.ReleaseChild();
				columnSlice.BlockProcessor = (isLastRow ? null : processor.CreateChild());
			}
			if (isLastRow || columnSlice.CurrentColumnSpan == 0 || (columnSlice.CurrentCell != null && columnSlice.CurrentCell.AllowClose))
			{
				columnSlice.CurrentCell = null;
			}
		}
		if (tableRow != null && tableRow.Count > 0)
		{
			gridTable.Add(tableRow);
		}
	}

	private BlockState HandleContents(BlockProcessor processor, GridTableState tableState, Table gridTable)
	{
		bool flag = processor.CurrentChar == '+';
		List<GridTableState.ColumnSlice> columnSlices = tableState.ColumnSlices;
		StringSlice line = processor.Line;
		SetColumnSpanState(columnSlices, line);
		if (!flag && !CanContinueRow(columnSlices))
		{
			TerminateCurrentRow(processor, tableState, gridTable, isLastRow: false);
		}
		int num = 0;
		while (num < columnSlices.Count)
		{
			GridTableState.ColumnSlice columnSlice = columnSlices[num];
			int num2 = num + columnSlice.CurrentColumnSpan;
			if (num2 == num)
			{
				break;
			}
			GridTableState.ColumnSlice columnSlice2 = ((num2 < columnSlices.Count) ? columnSlices[num2] : null);
			StringSlice stringSlice = line;
			stringSlice.Start = line.Start + columnSlice.Start + 1;
			if (columnSlice2 != null)
			{
				stringSlice.End = line.Start + columnSlice2.Start - 1;
			}
			else
			{
				int end = columnSlices[columnSlices.Count - 1].End;
				char c = line.PeekCharExtra(end);
				if (c == '|' || (flag && c == '+'))
				{
					stringSlice.End = line.Start + end - 1;
				}
				else if (line.PeekCharExtra(line.End - line.Start) == '|')
				{
					stringSlice.End = line.End - 1;
				}
			}
			stringSlice.TrimEnd();
			if (!flag || !IsRowSeparator(stringSlice))
			{
				if (columnSlice.CurrentCell == null)
				{
					columnSlice.CurrentCell = new TableCell(this)
					{
						ColumnSpan = columnSlice.CurrentColumnSpan,
						ColumnIndex = num,
						Column = columnSlice.Start,
						Line = processor.LineIndex,
						Span = new SourceSpan(line.Start + columnSlice.Start, line.Start + columnSlice.End)
					};
					GridTableState.ColumnSlice columnSlice3 = columnSlice;
					if (columnSlice3.BlockProcessor == null)
					{
						BlockProcessor blockProcessor = (columnSlice3.BlockProcessor = processor.CreateChild());
					}
					columnSlice.BlockProcessor.Open(columnSlice.CurrentCell);
				}
				columnSlice.BlockProcessor.LineIndex = processor.LineIndex;
				columnSlice.BlockProcessor.ProcessLinePart(stringSlice, stringSlice.Start - line.Start);
			}
			num = num2;
		}
		return BlockState.ContinueDiscard;
	}

	private static void SetColumnSpanState(List<GridTableState.ColumnSlice> columns, StringSlice line)
	{
		foreach (GridTableState.ColumnSlice column in columns)
		{
			column.PreviousColumnSpan = column.CurrentColumnSpan;
			column.CurrentColumnSpan = 0;
		}
		int num = -1;
		for (int i = 0; i < columns.Count; i++)
		{
			GridTableState.ColumnSlice columnSlice = columns[i];
			char c = line.PeekChar(columnSlice.Start);
			if (c == '|' || c == '+')
			{
				num = i;
			}
			if (num >= 0)
			{
				columns[num].CurrentColumnSpan++;
			}
		}
	}

	private static bool CanContinueRow(List<GridTableState.ColumnSlice> columns)
	{
		foreach (GridTableState.ColumnSlice column in columns)
		{
			if (column.PreviousColumnSpan != column.CurrentColumnSpan)
			{
				return false;
			}
		}
		return true;
	}

	private static void Undo(BlockProcessor processor, GridTableState tableState, Table gridTable)
	{
		ParagraphBlockParser? parser = processor.Parsers.FindExact<ParagraphBlockParser>();
		ContainerBlock parent = gridTable.Parent;
		processor.Discard(gridTable);
		ParagraphBlock paragraphBlock = new ParagraphBlock(parser)
		{
			Lines = tableState.Lines
		};
		parent.Add(paragraphBlock);
		processor.Open(paragraphBlock);
	}

	public override bool Close(BlockProcessor processor, Block block)
	{
		if (block is Table table)
		{
			GridTableState tableState = (GridTableState)block.GetData(typeof(GridTableState));
			TerminateCurrentRow(processor, tableState, table, isLastRow: true);
			if (!table.IsValid())
			{
				Undo(processor, tableState, table);
			}
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Tables;

public class PipeTableParser : InlineParser, IPostInlineProcessor
{
	private sealed class TableState
	{
		public bool IsInvalidTable { get; set; }

		public bool LineHasPipe { get; set; }

		public List<Inline> ColumnAndLineDelimiters { get; } = new List<Inline>();

		public List<TableCell> Cells { get; } = new List<TableCell>();

		public List<Inline> EndOfLines { get; } = new List<Inline>();
	}

	private readonly LineBreakInlineParser _lineBreakParser;

	public PipeTableOptions Options { get; }

	public PipeTableParser(LineBreakInlineParser lineBreakParser, PipeTableOptions? options = null)
	{
		_lineBreakParser = lineBreakParser ?? throw new ArgumentNullException("lineBreakParser");
		base.OpeningCharacters = new char[3] { '|', '\n', '\r' };
		Options = options ?? new PipeTableOptions();
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		if (!processor.Block.IsParagraphBlock)
		{
			return false;
		}
		char currentChar = slice.CurrentChar;
		TableState tableState = processor.ParserStates[base.Index] as TableState;
		bool flag = false;
		int sourcePosition = processor.GetSourcePosition(slice.Start, out var lineIndex, out var column);
		int localLineIndex = lineIndex - processor.LineIndex;
		if (tableState == null)
		{
			if (processor.Inline != null && (currentChar == '\n' || currentChar == '\r'))
			{
				return false;
			}
			if (processor.Inline == null)
			{
				flag = true;
			}
			tableState = new TableState();
			processor.ParserStates[base.Index] = tableState;
		}
		if (currentChar == '\n' || currentChar == '\r')
		{
			if (!flag && !tableState.LineHasPipe)
			{
				tableState.IsInvalidTable = true;
			}
			tableState.LineHasPipe = false;
			_lineBreakParser.Match(processor, ref slice);
			if (!flag)
			{
				tableState.ColumnAndLineDelimiters.Add(processor.Inline);
				tableState.EndOfLines.Add(processor.Inline);
			}
		}
		else
		{
			processor.Inline = new PipeTableDelimiterInline(this)
			{
				Span = new SourceSpan(sourcePosition, sourcePosition),
				Line = lineIndex,
				Column = column,
				LocalLineIndex = localLineIndex,
				IsClosed = true
			};
			tableState.LineHasPipe = true;
			slice.SkipChar();
			tableState.ColumnAndLineDelimiters.Add(processor.Inline);
		}
		return true;
	}

	public bool PostProcess(InlineProcessor state, Inline? root, Inline? lastChild, int postInlineProcessorIndex, bool isFinalProcessing)
	{
		ContainerInline containerInline = root as ContainerInline;
		TableState tableState = state.ParserStates[base.Index] as TableState;
		if (!isFinalProcessing)
		{
			if (containerInline == null || tableState == null)
			{
				return true;
			}
			Inline inline = containerInline.LastChild;
			List<PipeTableDelimiterInline> list = null;
			while (inline != null)
			{
				if (inline is PipeTableDelimiterInline item)
				{
					if (list == null)
					{
						list = new List<PipeTableDelimiterInline>();
					}
					list.Add(item);
				}
				if (inline == lastChild)
				{
					break;
				}
				inline = inline.PreviousSibling;
			}
			if (list != null)
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < list.Count; i++)
				{
					PipeTableDelimiterInline pipeTableDelimiterInline = list[i];
					pipeTableDelimiterInline.ReplaceByLiteral();
					List<Inline> columnAndLineDelimiters = tableState.ColumnAndLineDelimiters;
					int num = columnAndLineDelimiters.IndexOf(pipeTableDelimiterInline);
					if (i == 0)
					{
						flag = num > 0 && columnAndLineDelimiters[num - 1] is PipeTableDelimiterInline;
					}
					else if (i + 1 == list.Count)
					{
						flag2 = num + 1 < columnAndLineDelimiters.Count && columnAndLineDelimiters[num + 1] is PipeTableDelimiterInline;
					}
					tableState.ColumnAndLineDelimiters.Remove(pipeTableDelimiterInline);
				}
				if (!flag && !flag2)
				{
					tableState.LineHasPipe = false;
				}
			}
			return true;
		}
		state.ParserStates[base.Index] = null;
		if (tableState == null || containerInline == null || tableState.IsInvalidTable || !tableState.LineHasPipe)
		{
			if (tableState != null)
			{
				foreach (Inline columnAndLineDelimiter in tableState.ColumnAndLineDelimiters)
				{
					if (columnAndLineDelimiter is PipeTableDelimiterInline pipeTableDelimiterInline2)
					{
						pipeTableDelimiterInline2.ReplaceByLiteral();
					}
				}
			}
			return true;
		}
		List<Inline> columnAndLineDelimiters2 = tableState.ColumnAndLineDelimiters;
		List<TableColumnDefinition> list2 = FindHeaderRow(columnAndLineDelimiters2);
		if (Options.RequireHeaderSeparator && list2 == null)
		{
			foreach (Inline item3 in columnAndLineDelimiters2)
			{
				if (item3 is PipeTableDelimiterInline pipeTableDelimiterInline3)
				{
					pipeTableDelimiterInline3.ReplaceByLiteral();
				}
			}
			return true;
		}
		Table table = new Table();
		state.Block.TryGetAttributes()?.CopyTo(table.GetAttributes());
		List<TableCell> cells = tableState.Cells;
		cells.Clear();
		PromoteNestedPipesToRootLevel(columnAndLineDelimiters2, containerInline);
		Inline inline2 = columnAndLineDelimiters2[columnAndLineDelimiters2.Count - 1];
		if (!(inline2 is LineBreakInline))
		{
			while (inline2.NextSibling != null)
			{
				inline2 = inline2.NextSibling;
			}
			LineBreakInline lineBreakInline = new LineBreakInline();
			inline2.InsertAfter(lineBreakInline);
			columnAndLineDelimiters2.Add(lineBreakInline);
			tableState.EndOfLines.Add(lineBreakInline);
		}
		int num2 = 0;
		TableRow tableRow = null;
		TableRow tableRow2 = null;
		for (int j = 0; j < columnAndLineDelimiters2.Count; j++)
		{
			Inline inline3 = columnAndLineDelimiters2[j];
			PipeTableDelimiterInline pipeTableDelimiterInline4 = inline3 as PipeTableDelimiterInline;
			bool flag3 = inline3 is LineBreakInline;
			if (tableRow == null)
			{
				tableRow = new TableRow();
				if (tableRow2 == null)
				{
					tableRow2 = tableRow;
				}
				if (pipeTableDelimiterInline4 != null && (inline3.PreviousSibling == null || inline3.PreviousSibling is LineBreakInline))
				{
					inline3.Remove();
					if (table.Span.IsEmpty)
					{
						table.Span = inline3.Span;
						table.Line = inline3.Line;
						table.Column = inline3.Column;
					}
					continue;
				}
			}
			Inline inline4 = null;
			Inline inline5 = null;
			Inline previousSibling = inline3.PreviousSibling;
			while (previousSibling != null && !(previousSibling is LineBreakInline) && !(previousSibling is PipeTableDelimiterInline) && (!(previousSibling.GetType() == typeof(ContainerInline)) || previousSibling.Parent != null))
			{
				inline5 = previousSibling;
				if (inline4 == null)
				{
					inline4 = inline5;
				}
				previousSibling = previousSibling.PreviousSibling;
			}
			if (!flag3 || (inline5 != null && inline4 != null && (inline5 != inline4 || (!(inline5 is LineBreakInline) && (!(inline5 is LiteralInline literalInline) || !literalInline.Content.IsEmptyOrWhitespace())))))
			{
				TrimStart(inline5);
				TrimEnd(inline4);
				ContainerInline containerInline2 = new ContainerInline();
				Inline inline6 = inline5;
				while (inline6 != null && !IsLine(inline6) && !(inline6 is PipeTableDelimiterInline))
				{
					Inline nextSibling = inline6.NextSibling;
					if (inline6 is LiteralInline { Content: { IsEmpty: not false } })
					{
						inline6.Remove();
						inline6 = nextSibling;
						continue;
					}
					inline6.Remove();
					if (containerInline2.Span.IsEmpty)
					{
						containerInline2.Line = inline6.Line;
						containerInline2.Column = inline6.Column;
						containerInline2.Span = inline6.Span;
					}
					containerInline2.AppendChild(inline6);
					containerInline2.Span.End = inline6.Span.End;
					inline6 = nextSibling;
				}
				if (!flag3)
				{
					inline3.Remove();
					num2 = inline3.Span.End;
				}
				ParagraphBlock item2 = new ParagraphBlock
				{
					Span = containerInline2.Span,
					Line = containerInline2.Line,
					Column = containerInline2.Column,
					Inline = containerInline2
				};
				TableCell tableCell = new TableCell
				{
					Span = containerInline2.Span,
					Line = containerInline2.Line,
					Column = containerInline2.Column
				};
				tableCell.Add(item2);
				if (tableRow.Span.IsEmpty)
				{
					tableRow.Span = containerInline2.Span;
					tableRow.Line = containerInline2.Line;
					tableRow.Column = containerInline2.Column;
				}
				tableRow.Add(tableCell);
				cells.Add(tableCell);
			}
			if (flag3)
			{
				if (table.Span.IsEmpty)
				{
					table.Span = tableRow.Span;
					table.Line = tableRow.Line;
					table.Column = tableRow.Column;
				}
				table.Add(tableRow);
				tableRow = null;
			}
		}
		if (num2 > table.Span.End)
		{
			table.UpdateSpanEnd(num2);
		}
		foreach (Inline endOfLine in tableState.EndOfLines)
		{
			endOfLine.Remove();
		}
		TableRow tableRow3 = (TableRow)table[0];
		tableRow3.IsHeader = Options.RequireHeaderSeparator;
		if (list2 != null)
		{
			tableRow3.IsHeader = true;
			table.RemoveAt(1);
			table.ColumnDefinitions.AddRange(list2);
		}
		foreach (TableCell item4 in cells)
		{
			ParagraphBlock paragraphBlock = (ParagraphBlock)item4[0];
			state.PostProcessInlines(0, paragraphBlock.Inline, null, isFinalProcessing: true);
			if (paragraphBlock.Inline?.LastChild != null)
			{
				paragraphBlock.Inline.Span.End = paragraphBlock.Inline.LastChild.Span.End;
				paragraphBlock.UpdateSpanEnd(paragraphBlock.Inline.LastChild.Span.End);
			}
		}
		cells.Clear();
		if (Options.UseHeaderForColumnCount)
		{
			table.NormalizeUsingHeaderRow();
		}
		else
		{
			table.NormalizeUsingMaxWidth();
		}
		LeafBlock block = state.Block;
		ParagraphBlock leadingParagraph = block as ParagraphBlock;
		if (leadingParagraph != null)
		{
			ContainerInline inline7 = block.Inline;
			if (inline7 != null && inline7.FirstChild != null)
			{
				state.PostProcessInlines(0, leadingParagraph.Inline, null, isFinalProcessing: true);
				ContainerBlock parent = leadingParagraph.Parent;
				parent.ProcessInlinesEnd += delegate
				{
					parent.Insert(parent.IndexOf(leadingParagraph) + 1, table);
				};
				goto IL_07ea;
			}
		}
		state.BlockNew = table;
		goto IL_07ea;
		IL_07ea:
		return false;
	}

	private static bool ParseHeaderString(Inline? inline, out TableColumnAlign? align, out int delimiterCount)
	{
		align = TableColumnAlign.Left;
		delimiterCount = 0;
		if (!(inline is LiteralInline { Content: var slice }))
		{
			return false;
		}
		if (TableHelper.ParseColumnHeader(ref slice, '-', out align, out delimiterCount))
		{
			if (slice.CurrentChar != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private List<TableColumnDefinition>? FindHeaderRow(List<Inline> delimiters)
	{
		bool flag = false;
		int num = 0;
		List<TableColumnDefinition> list = null;
		for (int i = 0; i < delimiters.Count; i++)
		{
			if (!IsLine(delimiters[i]))
			{
				continue;
			}
			for (int j = i + 1; j < delimiters.Count; j++)
			{
				Inline inline = delimiters[j];
				Inline inline2 = ((j + 1 < delimiters.Count) ? delimiters[j + 1] : null);
				PipeTableDelimiterInline inline3 = inline as PipeTableDelimiterInline;
				if (j == i + 1 && IsStartOfLineColumnDelimiter(inline3))
				{
					continue;
				}
				if (IsLine(inline))
				{
					Inline inline4 = SkipTrailingWhitespace(inline.PreviousSibling);
					TableColumnAlign? align;
					int delimiterCount;
					if (inline4 is PipeTableDelimiterInline)
					{
						flag = list != null && list.Count > 0;
					}
					else if (TryParseHeaderColumn(inline4, out align, out delimiterCount))
					{
						if (list == null)
						{
							list = new List<TableColumnDefinition>();
						}
						num += delimiterCount;
						list.Add(new TableColumnDefinition
						{
							Alignment = align,
							Width = delimiterCount
						});
						flag = true;
					}
					break;
				}
				if (!TryParseHeaderColumn(inline.PreviousSibling, out var align2, out var delimiterCount2))
				{
					break;
				}
				if (list == null)
				{
					list = new List<TableColumnDefinition>();
				}
				num += delimiterCount2;
				list.Add(new TableColumnDefinition
				{
					Alignment = align2,
					Width = delimiterCount2
				});
				if (inline2 == null)
				{
					Inline nextSibling = inline.NextSibling;
					TableColumnAlign? align3;
					int delimiterCount3;
					if (IsNullOrSpace(nextSibling))
					{
						flag = true;
					}
					else if (TryParseHeaderColumn(nextSibling, out align3, out delimiterCount3))
					{
						num += delimiterCount3;
						flag = true;
						list.Add(new TableColumnDefinition
						{
							Alignment = align3,
							Width = delimiterCount3
						});
					}
					break;
				}
			}
			break;
		}
		if (!flag || list == null || num == 0)
		{
			return null;
		}
		if (Options.InferColumnWidthsFromSeparator)
		{
			foreach (TableColumnDefinition item in list)
			{
				item.Width = item.Width * 100f / (float)num;
			}
		}
		else
		{
			foreach (TableColumnDefinition item2 in list)
			{
				item2.Width = 0f;
			}
		}
		return list;
	}

	private static bool TryParseHeaderColumn(Inline? inline, out TableColumnAlign? align, out int delimiterCount)
	{
		align = null;
		delimiterCount = 0;
		if (inline == null || inline is PipeTableDelimiterInline)
		{
			return false;
		}
		if (inline is LiteralInline literalInline && literalInline.Content.IsEmptyOrWhitespace())
		{
			return false;
		}
		return ParseHeaderString(inline, out align, out delimiterCount);
	}

	private static Inline? SkipTrailingWhitespace(Inline? inline)
	{
		while (inline is LiteralInline literalInline && literalInline.Content.IsEmptyOrWhitespace())
		{
			inline = inline.PreviousSibling;
		}
		return inline;
	}

	private static bool IsLine(Inline inline)
	{
		return inline is LineBreakInline;
	}

	private static bool IsStartOfLineColumnDelimiter(Inline? inline)
	{
		if (inline == null)
		{
			return false;
		}
		Inline previousSibling = inline.PreviousSibling;
		if (previousSibling == null)
		{
			return true;
		}
		if (previousSibling is LiteralInline literalInline)
		{
			if (!literalInline.Content.IsEmptyOrWhitespace())
			{
				return false;
			}
			previousSibling = previousSibling.PreviousSibling;
		}
		if (previousSibling != null)
		{
			return IsLine(previousSibling);
		}
		return true;
	}

	private static void TrimStart(Inline? inline)
	{
		while (inline is ContainerInline containerInline && !(containerInline is DelimiterInline))
		{
			inline = containerInline.FirstChild;
		}
		if (inline is LiteralInline literalInline)
		{
			literalInline.Content.TrimStart();
		}
	}

	private static void TrimEnd(Inline? inline)
	{
		while (inline is ContainerInline containerInline && !(inline is PipeTableDelimiterInline))
		{
			inline = containerInline.LastChild;
		}
		if (inline is LiteralInline literalInline)
		{
			literalInline.Content.TrimEnd();
		}
	}

	private static bool IsNullOrSpace(Inline? inline)
	{
		if (inline == null)
		{
			return true;
		}
		if (inline is LiteralInline literalInline)
		{
			return literalInline.Content.IsEmptyOrWhitespace();
		}
		return false;
	}

	private static void PromoteNestedPipesToRootLevel(List<Inline> delimiters, ContainerInline root)
	{
		for (int i = 0; i < delimiters.Count; i++)
		{
			Inline inline = delimiters[i];
			bool flag = inline is PipeTableDelimiterInline;
			bool flag2 = inline is LineBreakInline;
			if ((flag || flag2) && inline.Parent != root)
			{
				ContainerInline parent = inline.Parent;
				while (parent?.Parent != null && parent.Parent != root)
				{
					parent = parent.Parent;
				}
				if (parent != null && parent.Parent == root)
				{
					SplitContainerAtDelimiter(inline, parent);
				}
			}
		}
	}

	private static void SplitContainerAtDelimiter(Inline delimiter, Inline ancestor)
	{
		ContainerInline parent = delimiter.Parent;
		if (parent == null)
		{
			return;
		}
		List<Inline> list = new List<Inline>();
		for (Inline nextSibling = delimiter.NextSibling; nextSibling != null; nextSibling = nextSibling.NextSibling)
		{
			list.Add(nextSibling);
		}
		foreach (Inline item in list)
		{
			item.Remove();
		}
		delimiter.Remove();
		ancestor.InsertAfter(delimiter);
		if (list.Count <= 0)
		{
			return;
		}
		ContainerInline containerInline = CreateMatchingContainer(parent);
		foreach (Inline item2 in list)
		{
			containerInline.AppendChild(item2);
		}
		delimiter.InsertAfter(containerInline);
	}

	private static ContainerInline CreateMatchingContainer(ContainerInline source)
	{
		return new ContainerInline
		{
			Span = source.Span,
			Line = source.Line,
			Column = source.Column
		};
	}
}

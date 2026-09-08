using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class BlockProcessor
{
	private sealed class BlockProcessorCache : ObjectCache<BlockProcessor>
	{
		protected override BlockProcessor NewInstance()
		{
			return new BlockProcessor();
		}

		protected override void Reset(BlockProcessor instance)
		{
			instance.Reset();
		}
	}

	private int currentStackIndex;

	private int originalLineStart;

	public StringSlice Line;

	private static readonly BlockProcessorCache _cache = new BlockProcessorCache();

	public bool SkipFirstUnwindSpace { get; set; }

	public Stack<Block> NewBlocks { get; } = new Stack<Block>();

	public BlockParserList Parsers { get; private set; }

	public MarkdownParserContext? Context { get; private set; }

	public ContainerBlock? CurrentContainer { get; private set; }

	public Block? CurrentBlock { get; private set; }

	public Block? LastBlock { get; private set; }

	public Block? NextContinue
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			int num = currentStackIndex + 1;
			if (num >= OpenedBlocks.Count)
			{
				return null;
			}
			return OpenedBlocks[num].Block;
		}
	}

	public MarkdownDocument Document { get; private set; }

	public int CurrentLineStartPosition { get; private set; }

	public int LineIndex { get; set; }

	public bool IsBlankLine => Line.IsEmpty;

	public char CurrentChar => Line.CurrentChar;

	public int Column { get; set; }

	public int Start => Line.Start;

	public int Indent => Column - ColumnBeforeIndent;

	public bool IsCodeIndent => Indent >= 4;

	public int ColumnBeforeIndent { get; private set; }

	public int StartBeforeIndent { get; private set; }

	public bool IsLazy { get; private set; }

	private List<BlockWrapper> OpenedBlocks { get; } = new List<BlockWrapper>();

	private bool ContinueProcessingLine { get; set; }

	public int TriviaStart { get; set; }

	public List<StringSlice>? LinesBefore { get; set; }

	public bool TrackTrivia { get; private set; }

	public BlockProcessor(MarkdownDocument document, BlockParserList parsers, MarkdownParserContext? context, bool trackTrivia = false)
	{
		Setup(document, parsers, context, trackTrivia);
		document.IsOpen = true;
		Open(document);
	}

	private BlockProcessor()
	{
	}

	public StringSlice UseTrivia(int end)
	{
		StringSlice result = new StringSlice(Line.Text, TriviaStart, end);
		TriviaStart = end + 1;
		return result;
	}

	public List<StringSlice>? TakeLinesBefore()
	{
		List<StringSlice>? linesBefore = LinesBefore;
		LinesBefore = null;
		return linesBefore;
	}

	public ContainerBlock GetCurrentContainerOpened()
	{
		ContainerBlock containerBlock = CurrentContainer;
		while (containerBlock != null && !containerBlock.IsOpen)
		{
			containerBlock = containerBlock.Parent;
		}
		return containerBlock;
	}

	public char NextChar()
	{
		if (Line.CurrentChar == '\t')
		{
			Column = CharHelper.AddTab(Column);
		}
		else
		{
			Column++;
		}
		return Line.NextChar();
	}

	public void NextColumn()
	{
		if (Line.CurrentChar == '\t' && CharHelper.IsAcrossTab(Column))
		{
			Column++;
			return;
		}
		Line.NextChar();
		Column++;
	}

	public char PeekChar(int offset)
	{
		return Line.PeekChar(offset);
	}

	public void RestartIndent()
	{
		StartBeforeIndent = Start;
		ColumnBeforeIndent = Column;
	}

	public void ParseIndent()
	{
		char c = CurrentChar;
		int startBeforeIndent = StartBeforeIndent;
		int start = Start;
		int columnBeforeIndent = ColumnBeforeIndent;
		int column = Column;
		while (true)
		{
			switch (c)
			{
			case '\t':
				Column = CharHelper.AddTab(Column);
				break;
			case ' ':
				Column++;
				break;
			default:
				if (column == Column)
				{
					StartBeforeIndent = startBeforeIndent;
					ColumnBeforeIndent = columnBeforeIndent;
				}
				else
				{
					StartBeforeIndent = start;
					ColumnBeforeIndent = column;
				}
				return;
			}
			c = Line.NextChar();
		}
	}

	public void GoToColumn(int newColumn)
	{
		if (newColumn >= ColumnBeforeIndent)
		{
			Line.Start = StartBeforeIndent;
			Column = ColumnBeforeIndent;
		}
		else
		{
			Line.Start = originalLineStart;
			Column = 0;
			ColumnBeforeIndent = 0;
			StartBeforeIndent = originalLineStart;
		}
		while (Line.Start <= Line.End && Column < newColumn)
		{
			char c = Line.Text[Line.Start];
			if (c == '\t')
			{
				Column = CharHelper.AddTab(Column);
			}
			else
			{
				if (!c.IsSpaceOrTab())
				{
					ColumnBeforeIndent = Column + 1;
					StartBeforeIndent = Line.Start + 1;
				}
				Column++;
			}
			Line.Start++;
		}
		if (Column > newColumn)
		{
			Column = newColumn;
			if (Line.Start > 0)
			{
				Line.Start--;
			}
		}
	}

	public void UnwindAllIndents()
	{
		int start = Line.Start;
		while (Line.Start > originalLineStart)
		{
			char c = Line.PeekCharAbsolute(Line.Start - 1);
			if ((TrackTrivia && SkipFirstUnwindSpace && Line.Start == TriviaStart) || c == '\0' || !c.IsSpaceOrTab())
			{
				break;
			}
			Line.Start--;
		}
		int start2 = Line.Start;
		if (start == start2)
		{
			return;
		}
		Line.Start = originalLineStart;
		Column = 0;
		ColumnBeforeIndent = 0;
		StartBeforeIndent = originalLineStart;
		while (Line.Start < start2)
		{
			char c2 = Line.Text[Line.Start];
			if (c2 == '\t')
			{
				Column = CharHelper.AddTab(Column);
			}
			else
			{
				if (!c2.IsSpaceOrTab())
				{
					ColumnBeforeIndent = Column + 1;
					StartBeforeIndent = Line.Start + 1;
				}
				Column++;
			}
			Line.Start++;
		}
		ColumnBeforeIndent = Column;
		StartBeforeIndent = Start;
	}

	public void GoToCodeIndent(int columnOffset = 0)
	{
		GoToColumn(ColumnBeforeIndent + 4 + columnOffset);
	}

	public void Open(Block block)
	{
		if (block == null)
		{
			ThrowHelper.ArgumentNullException("block");
		}
		if (!block.IsOpen)
		{
			ThrowHelper.ArgumentException("The block must be opened", "block");
		}
		OpenedBlocks.Add(block);
	}

	public void Close(Block block)
	{
		for (int num = OpenedBlocks.Count - 1; num >= 0; num--)
		{
			if (OpenedBlocks[num].Block == block)
			{
				for (int num2 = OpenedBlocks.Count - 1; num2 >= num; num2--)
				{
					Close(num2);
				}
				break;
			}
		}
	}

	public void Discard(Block block)
	{
		TryDiscard(block);
	}

	public bool TryDiscard(Block block)
	{
		if (block == null)
		{
			ThrowHelper.ArgumentNullException("block");
		}
		for (int num = OpenedBlocks.Count - 1; num >= 1; num--)
		{
			if (OpenedBlocks[num].Block == block)
			{
				block.Parent.Remove(block);
				OpenedBlocks.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	public void ProcessLine(StringSlice newLine)
	{
		CurrentLineStartPosition = newLine.Start;
		Document.LineStartIndexes?.Add(CurrentLineStartPosition);
		ContinueProcessingLine = true;
		ResetLine(newLine, 0);
		Process();
		LineIndex++;
	}

	public void ProcessLinePart(StringSlice line, int column)
	{
		CurrentLineStartPosition = line.Start - column;
		ContinueProcessingLine = true;
		ResetLine(line, column);
		Process();
	}

	private void Process()
	{
		TryContinueBlocks();
		TryOpenBlocks();
		CloseAll(force: false);
	}

	public bool IsOpen(Block block)
	{
		if (block == null)
		{
			ThrowHelper.ArgumentNullException("block");
		}
		return OpenedBlocks.Contains(block);
	}

	private void Close(int index)
	{
		Block block = OpenedBlocks[index].Block;
		if (block.Parser != null)
		{
			if (!block.Parser.Close(this, block))
			{
				block.Parent?.Remove(block);
				if (block.IsLeafBlock)
				{
					Unsafe.As<LeafBlock>(block).Lines.Release();
				}
			}
			else
			{
				block.Parser.GetClosedEvent?.Invoke(this, block);
			}
		}
		OpenedBlocks.RemoveAt(index);
	}

	internal void CloseAll(bool force)
	{
		for (int num = OpenedBlocks.Count - 1; num >= 1; num--)
		{
			Block block = OpenedBlocks[num].Block;
			if (!force && block.IsOpen)
			{
				break;
			}
			if (TrackTrivia)
			{
				List<StringSlice> linesBefore = LinesBefore;
				if (linesBefore != null && linesBefore.Count > 0)
				{
					if (LinesBefore.Count == 1)
					{
						Block block2 = block;
						if (block2.LinesAfter == null)
						{
							linesBefore = (block2.LinesAfter = new List<StringSlice>());
						}
						List<StringSlice> list2 = TakeLinesBefore();
						if (list2 != null)
						{
							block.LinesAfter.AddRange(list2);
						}
					}
					else
					{
						Block block3 = Block.FindRootMostContainerParent(block);
						Block block2 = block3;
						if (block2.LinesAfter == null)
						{
							linesBefore = (block2.LinesAfter = new List<StringSlice>());
						}
						List<StringSlice> list4 = TakeLinesBefore();
						if (list4 != null)
						{
							block3.LinesAfter.AddRange(list4);
						}
					}
				}
			}
			Close(num);
		}
		UpdateLastBlockAndContainer();
	}

	private void OpenAll()
	{
		for (int i = 1; i < OpenedBlocks.Count; i++)
		{
			OpenedBlocks[i].Block.IsOpen = true;
		}
	}

	private void UpdateLastBlockAndContainer(int stackIndex = -1)
	{
		List<BlockWrapper> openedBlocks = OpenedBlocks;
		currentStackIndex = ((stackIndex < 0) ? (openedBlocks.Count - 1) : stackIndex);
		Block block = null;
		for (int num = openedBlocks.Count - 1; num >= 0; num--)
		{
			Block block2 = openedBlocks[num].Block;
			if (block == null)
			{
				block = block2;
			}
			if (block2.IsContainerBlock)
			{
				ContainerBlock containerBlock = (CurrentContainer = Unsafe.As<ContainerBlock>(block2));
				LastBlock = containerBlock.LastChild;
				CurrentBlock = block;
				return;
			}
		}
		CurrentBlock = block;
		LastBlock = null;
	}

	private void TryContinueBlocks()
	{
		IsLazy = false;
		for (int i = 1; i < OpenedBlocks.Count; i++)
		{
			OpenedBlocks[i].Block.IsOpen = false;
		}
		for (int j = 1; j < OpenedBlocks.Count; j++)
		{
			Block block = OpenedBlocks[j].Block;
			ParseIndent();
			if (block.IsParagraphBlock)
			{
				break;
			}
			BlockParser? parser = block.Parser;
			UpdateLastBlockAndContainer(j);
			BlockState blockState = parser.TryContinue(this, block);
			switch (blockState)
			{
			case BlockState.Skip:
				continue;
			case BlockState.None:
				return;
			}
			RestartIndent();
			if (j >= OpenedBlocks.Count)
			{
				j = OpenedBlocks.Count - 1;
			}
			if (j + 1 < OpenedBlocks.Count && NewBlocks.Count > 0)
			{
				ThrowHelper.InvalidOperationException("A pending parser cannot add a new block when it is not the last pending block");
			}
			if (block.IsLeafBlock && NewBlocks.Count == 0)
			{
				ContinueProcessingLine = false;
				if (!blockState.IsDiscard())
				{
					if (TrackTrivia && block is FencedCodeBlock && block.Parent is ListItemBlock)
					{
						UnwindAllIndents();
					}
					Unsafe.As<LeafBlock>(block).AppendLine(ref Line, Column, LineIndex, CurrentLineStartPosition, TrackTrivia);
				}
			}
			block.IsOpen = blockState == BlockState.Continue || blockState == BlockState.ContinueDiscard;
			if (blockState == BlockState.BreakDiscard)
			{
				if (Line.IsEmpty && TrackTrivia)
				{
					if (LinesBefore == null)
					{
						List<StringSlice> list = (LinesBefore = new List<StringSlice>());
					}
					StringSlice item = new StringSlice(Line.Text, TriviaStart, Line.Start - 1, Line.NewLine);
					LinesBefore.Add(item);
					Line.Start = StartBeforeIndent;
				}
				ContinueProcessingLine = false;
				return;
			}
			bool num = j == OpenedBlocks.Count - 1;
			if (ContinueProcessingLine)
			{
				ProcessNewBlocks(blockState, allowClosing: false);
			}
			if (num || !ContinueProcessingLine)
			{
				return;
			}
		}
	}

	private void TryOpenBlocks()
	{
		int num = -1;
		while (ContinueProcessingLine)
		{
			if (num == Start)
			{
				ThrowHelper.InvalidOperationException($"The parser is in an invalid infinite loop while trying to parse blocks at line [{LineIndex}] with line [{Line}]");
			}
			num = Start;
			ParseIndent();
			BlockParser[] parsersForOpeningCharacter = Parsers.GetParsersForOpeningCharacter(CurrentChar);
			BlockParser[] globalParsers = Parsers.GlobalParsers;
			if (parsersForOpeningCharacter != null && TryOpenBlocks(parsersForOpeningCharacter))
			{
				RestartIndent();
				continue;
			}
			if (globalParsers != null && ContinueProcessingLine && TryOpenBlocks(globalParsers))
			{
				RestartIndent();
				continue;
			}
			break;
		}
	}

	private bool TryOpenBlocks(BlockParser[] parsers)
	{
		for (int i = 0; i < parsers.Length; i++)
		{
			IsLazy = false;
			BlockParser blockParser = parsers[i];
			if (Line.IsEmpty)
			{
				if (TrackTrivia)
				{
					if (LinesBefore == null)
					{
						List<StringSlice> list = (LinesBefore = new List<StringSlice>());
					}
					StringSlice item = new StringSlice(Line.Text, TriviaStart, Line.Start - 1, Line.NewLine);
					LinesBefore.Add(item);
					Line.Start = StartBeforeIndent;
				}
				ContinueProcessingLine = false;
				break;
			}
			UpdateLastBlockAndContainer();
			Block currentBlock = CurrentBlock;
			if (!blockParser.CanInterrupt(this, currentBlock))
			{
				continue;
			}
			IsLazy = currentBlock.IsParagraphBlock && blockParser is ParagraphBlockParser;
			BlockState blockState = (IsLazy ? blockParser.TryContinue(this, currentBlock) : blockParser.TryOpen(this));
			if (blockState == BlockState.None)
			{
				if (IsLazy && IsBlankLine)
				{
					ContinueProcessingLine = false;
					break;
				}
				continue;
			}
			UpdateLastBlockAndContainer();
			if (IsLazy)
			{
				Block currentBlock2 = CurrentBlock;
				if (currentBlock2 != null && currentBlock2.IsParagraphBlock)
				{
					if (!blockState.IsDiscard())
					{
						if (TrackTrivia)
						{
							UnwindAllIndents();
						}
						Unsafe.As<ParagraphBlock>(currentBlock2).AppendLine(ref Line, Column, LineIndex, CurrentLineStartPosition, TrackTrivia);
					}
					if (TrackTrivia && currentBlock2.Parent is QuoteBlock quoteBlock)
					{
						StringSlice triviaAfter = UseTrivia(Start - 1);
						quoteBlock.QuoteLines.Last().TriviaAfter = triviaAfter;
					}
					OpenAll();
					ContinueProcessingLine = false;
					break;
				}
			}
			if (NewBlocks.Count == 0 && blockState == BlockState.BreakDiscard)
			{
				ContinueProcessingLine = false;
				break;
			}
			ProcessNewBlocks(blockState, allowClosing: true);
			return ContinueProcessingLine;
		}
		IsLazy = false;
		return false;
	}

	private void ProcessNewBlocks(BlockState result, bool allowClosing)
	{
		Stack<Block> newBlocks = NewBlocks;
		while (newBlocks.Count > 0)
		{
			Block block = newBlocks.Pop();
			if (block.Parser == null)
			{
				ThrowHelper.InvalidOperationException($"The new block [{block.GetType()}] must have a valid Parser property");
			}
			block.Line = LineIndex;
			if (block.IsLeafBlock)
			{
				if (!result.IsDiscard())
				{
					if (TrackTrivia && (block.IsParagraphBlock || block is HtmlBlock))
					{
						UnwindAllIndents();
					}
					Unsafe.As<LeafBlock>(block).AppendLine(ref Line, Column, LineIndex, CurrentLineStartPosition, TrackTrivia);
				}
				if (newBlocks.Count > 0)
				{
					ThrowHelper.InvalidOperationException("The NewBlocks is not empty. This is happening if a LeafBlock is not the last to be pushed");
				}
			}
			if (allowClosing)
			{
				CloseAll(force: false);
			}
			if (block.Parent == null)
			{
				UpdateLastBlockAndContainer();
				CurrentContainer.Add(block);
			}
			block.IsOpen = result.IsContinue();
			OpenedBlocks.Add(block);
			if (block.IsLeafBlock)
			{
				ContinueProcessingLine = false;
				return;
			}
		}
		ContinueProcessingLine = !result.IsDiscard();
	}

	private void ResetLine(StringSlice newLine, int column)
	{
		Line = newLine;
		Column = column;
		ColumnBeforeIndent = 0;
		StartBeforeIndent = Start;
		originalLineStart = newLine.Start - column;
		TriviaStart = newLine.Start;
	}

	[MemberNotNull(new string[] { "Document", "Parsers" })]
	internal void Setup(MarkdownDocument document, BlockParserList parsers, MarkdownParserContext? context, bool trackTrivia)
	{
		if (document == null)
		{
			ThrowHelper.ArgumentNullException("document");
		}
		if (parsers == null)
		{
			ThrowHelper.ArgumentNullException("parsers");
		}
		Document = document;
		Parsers = parsers;
		Context = context;
		TrackTrivia = trackTrivia;
	}

	private void Reset()
	{
		Document = null;
		Parsers = null;
		Context = null;
		CurrentContainer = null;
		CurrentBlock = null;
		LastBlock = null;
		TrackTrivia = false;
		SkipFirstUnwindSpace = false;
		ContinueProcessingLine = false;
		IsLazy = false;
		currentStackIndex = 0;
		originalLineStart = 0;
		CurrentLineStartPosition = 0;
		ColumnBeforeIndent = 0;
		StartBeforeIndent = 0;
		LineIndex = 0;
		Column = 0;
		TriviaStart = 0;
		Line = StringSlice.Empty;
		NewBlocks.Clear();
		OpenedBlocks.Clear();
		LinesBefore = null;
	}

	public BlockProcessor CreateChild()
	{
		return Rent(Document, Parsers, Context, TrackTrivia);
	}

	public void ReleaseChild()
	{
		Release(this);
	}

	internal static BlockProcessor Rent(MarkdownDocument document, BlockParserList parsers, MarkdownParserContext? context, bool trackTrivia)
	{
		BlockProcessor blockProcessor = _cache.Get();
		blockProcessor.Setup(document, parsers, context, trackTrivia);
		return blockProcessor;
	}

	internal static void Release(BlockProcessor processor)
	{
		_cache.Release(processor);
	}
}

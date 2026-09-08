using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Markdig.Helpers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers;

public class InlineProcessor
{
	private sealed class InlineProcessorCache : ObjectCache<InlineProcessor>
	{
		protected override InlineProcessor NewInstance()
		{
			return new InlineProcessor();
		}

		protected override void Reset(InlineProcessor instance)
		{
			instance.Reset();
		}
	}

	private readonly List<StringLineGroup.LineOffset> lineOffsets = new List<StringLineGroup.LineOffset>();

	private int previousSliceOffset;

	private int previousLineIndexForSliceOffset;

	internal ContainerBlock? PreviousContainerToReplace;

	internal ContainerBlock? NewContainerToReplace;

	private static readonly InlineProcessorCache _cache = new InlineProcessorCache();

	public LeafBlock? Block { get; private set; }

	public bool PreciseSourceLocation { get; private set; }

	public Block? BlockNew { get; set; }

	public Inline? Inline { get; set; }

	public ContainerInline? Root { get; internal set; }

	public InlineParserList Parsers { get; private set; }

	public MarkdownParserContext? Context { get; private set; }

	public MarkdownDocument Document { get; private set; }

	public int LineIndex { get; private set; }

	public object[] ParserStates { get; private set; }

	public TextWriter? DebugLog { get; set; }

	public bool TrackTrivia { get; private set; }

	public LiteralInlineParser LiteralInlineParser { get; } = new LiteralInlineParser();

	public InlineProcessor(MarkdownDocument document, InlineParserList parsers, bool preciseSourcelocation, MarkdownParserContext? context, bool trackTrivia = false)
	{
		Setup(document, parsers, preciseSourcelocation, context, trackTrivia);
	}

	private InlineProcessor()
	{
	}

	public SourceSpan GetSourcePositionFromLocalSpan(SourceSpan span)
	{
		if (span.IsEmpty)
		{
			return SourceSpan.Empty;
		}
		return new SourceSpan(GetSourcePosition(span.Start), GetSourcePosition(span.End));
	}

	public int GetSourcePosition(int sliceOffset, out int lineIndex, out int column)
	{
		column = 0;
		lineIndex = ((sliceOffset >= previousSliceOffset) ? previousLineIndexForSliceOffset : 0);
		int result = 0;
		if (PreciseSourceLocation)
		{
			while (lineIndex < lineOffsets.Count)
			{
				StringLineGroup.LineOffset lineOffset = lineOffsets[lineIndex];
				if (sliceOffset <= lineOffset.End)
				{
					previousSliceOffset = lineOffset.Start;
					int num = sliceOffset - previousSliceOffset;
					column = lineOffset.Column + num;
					result = lineOffset.LinePosition + num + lineOffset.Offset;
					previousLineIndexForSliceOffset = lineIndex;
					lineIndex += LineIndex;
					break;
				}
				lineIndex++;
			}
		}
		return result;
	}

	public int GetSourcePosition(int sliceOffset)
	{
		if (PreciseSourceLocation)
		{
			for (int i = ((sliceOffset >= previousSliceOffset) ? previousLineIndexForSliceOffset : 0); i < lineOffsets.Count; i++)
			{
				StringLineGroup.LineOffset lineOffset = lineOffsets[i];
				if (sliceOffset <= lineOffset.End)
				{
					previousLineIndexForSliceOffset = i;
					previousSliceOffset = lineOffset.Start;
					return sliceOffset - lineOffset.Start + lineOffset.LinePosition + lineOffset.Offset;
				}
			}
		}
		return 0;
	}

	public void ReplaceParentContainer(ContainerBlock previousParentContainer, ContainerBlock newParentContainer)
	{
		if (previousParentContainer == null)
		{
			ThrowHelper.ArgumentNullException("previousParentContainer");
		}
		if (newParentContainer == null)
		{
			ThrowHelper.ArgumentNullException("newParentContainer");
		}
		if (PreviousContainerToReplace != null)
		{
			throw new InvalidOperationException("A block is already being replaced");
		}
		PreviousContainerToReplace = previousParentContainer;
		NewContainerToReplace = newParentContainer;
	}

	public void ProcessInlineLeaf(LeafBlock leafBlock)
	{
		if (leafBlock == null)
		{
			ThrowHelper.ArgumentNullException_leafBlock();
		}
		PreviousContainerToReplace = null;
		NewContainerToReplace = null;
		Array.Clear(ParserStates, 0, ParserStates.Length);
		Root = new ContainerInline
		{
			IsClosed = false
		};
		leafBlock.Inline = Root;
		Inline = null;
		Block = leafBlock;
		BlockNew = null;
		LineIndex = leafBlock.Line;
		previousSliceOffset = 0;
		previousLineIndexForSliceOffset = 0;
		lineOffsets.Clear();
		StringSlice slice = leafBlock.Lines.ToSlice(lineOffsets);
		int end = slice.End;
		leafBlock.Lines.Release();
		int num = -1;
		while (!slice.IsEmpty)
		{
			if (num == slice.Start)
			{
				ThrowHelper.InvalidOperationException("The parser is in an invalid infinite loop while trying to parse inlines for block [" + leafBlock.GetType().Name + "] at position (" + leafBlock.ToPositionText());
			}
			num = slice.Start;
			char currentChar = slice.CurrentChar;
			StringSlice stringSlice = slice;
			InlineParser[] parsersForOpeningCharacter = Parsers.GetParsersForOpeningCharacter(currentChar);
			if (parsersForOpeningCharacter != null)
			{
				int num2 = 0;
				while (num2 < parsersForOpeningCharacter.Length)
				{
					slice = stringSlice;
					if (!parsersForOpeningCharacter[num2].Match(this, ref slice))
					{
						num2++;
						continue;
					}
					goto IL_016f;
				}
			}
			parsersForOpeningCharacter = Parsers.GlobalParsers;
			if (parsersForOpeningCharacter != null)
			{
				int num3 = 0;
				while (num3 < parsersForOpeningCharacter.Length)
				{
					slice = stringSlice;
					if (!parsersForOpeningCharacter[num3].Match(this, ref slice))
					{
						num3++;
						continue;
					}
					goto IL_016f;
				}
			}
			slice = stringSlice;
			LiteralInlineParser.Match(this, ref slice);
			goto IL_016f;
			IL_016f:
			Inline inline = Inline;
			if (inline != null)
			{
				if (inline.Parent != null)
				{
					continue;
				}
				ContainerInline containerInline = FindLastContainer();
				if (containerInline != inline)
				{
					containerInline.AppendChild(inline);
				}
				if (containerInline == Root)
				{
					if (containerInline.Span.IsEmpty)
					{
						containerInline.Span = inline.Span;
					}
					containerInline.Span.End = inline.Span.End;
				}
			}
			else
			{
				ContainerInline containerInline2 = FindLastContainer();
				Inline = ((containerInline2.LastChild is LeafInline) ? containerInline2.LastChild : containerInline2);
				if (Inline == Root)
				{
					Inline = null;
				}
			}
		}
		if (TrackTrivia && !(leafBlock is HeadingBlock))
		{
			NewLine newLine = leafBlock.NewLine;
			if (newLine != NewLine.None)
			{
				int sourcePosition = GetSourcePosition(end + 1, out var lineIndex, out var column);
				leafBlock.Inline.AppendChild(new LineBreakInline
				{
					NewLine = newLine,
					Line = lineIndex,
					Column = column,
					Span = 
					{
						Start = sourcePosition,
						End = sourcePosition + ((newLine == NewLine.CarriageReturnLineFeed) ? 1 : 0)
					}
				});
			}
		}
		Inline = null;
		PostProcessInlines(0, Root, null, isFinalProcessing: true);
		if (leafBlock.Inline.LastChild != null)
		{
			leafBlock.Inline.Span.End = leafBlock.Inline.LastChild.Span.End;
			leafBlock.UpdateSpanEnd(leafBlock.Inline.Span.End);
		}
	}

	public void PostProcessInlines(int startingIndex, Inline? root, Inline? lastChild, bool isFinalProcessing)
	{
		for (int i = startingIndex; i < Parsers.PostInlineProcessors.Length && Parsers.PostInlineProcessors[i].PostProcess(this, root, lastChild, i, isFinalProcessing); i++)
		{
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TState GetParserState<TState>(InlineParser parser, Func<TState> factory) where TState : class
	{
		if (parser == null)
		{
			ThrowHelper.ArgumentNullException("parser");
		}
		if (factory == null)
		{
			ThrowHelper.ArgumentNullException("factory");
		}
		ref object reference = ref ParserStates[parser.Index];
		if (reference is TState result)
		{
			return result;
		}
		TState val = factory();
		if (val == null)
		{
			ThrowHelper.InvalidOperationException($"The state factory for [{typeof(TState)}] returned null");
		}
		reference = val;
		return val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TState GetParserState<TState>(InlineParser parser) where TState : class, new()
	{
		return GetParserState(parser, () => new TState());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Emit(Inline inline)
	{
		if (inline == null)
		{
			ThrowHelper.ArgumentNullException("inline");
		}
		if (inline.Parent != null)
		{
			ThrowHelper.ArgumentException("Inline has already a parent", "inline");
		}
		ContainerInline containerInline = FindLastContainer();
		containerInline.AppendChild(inline);
		if (containerInline == Root && !inline.Span.IsEmpty)
		{
			if (containerInline.Span.IsEmpty)
			{
				containerInline.Span = inline.Span;
			}
			else
			{
				if (inline.Span.Start < containerInline.Span.Start)
				{
					containerInline.Span.Start = inline.Span.Start;
				}
				if (inline.Span.End > containerInline.Span.End)
				{
					containerInline.Span.End = inline.Span.End;
				}
			}
			Block?.UpdateSpanToInclude(inline.Span);
		}
		Inline = inline;
	}

	private ContainerInline FindLastContainer()
	{
		ContainerInline containerInline = Block.Inline;
		int num = 0;
		while (true)
		{
			Inline lastChild = containerInline.LastChild;
			if (lastChild == null || !lastChild.IsContainerInline || lastChild.IsClosed)
			{
				break;
			}
			containerInline = Unsafe.As<ContainerInline>(lastChild);
			num++;
		}
		ThrowHelper.CheckDepthLimit(num, useLargeLimit: true);
		return containerInline;
	}

	[MemberNotNull(new string[] { "Document", "Parsers", "ParserStates" })]
	private void Setup(MarkdownDocument document, InlineParserList parsers, bool preciseSourcelocation, MarkdownParserContext? context, bool trackTrivia)
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
		PreciseSourceLocation = preciseSourcelocation;
		TrackTrivia = trackTrivia;
		if (ParserStates == null || ParserStates.Length < Parsers.Count)
		{
			ParserStates = new object[Parsers.Count];
		}
	}

	private void Reset()
	{
		Block = null;
		BlockNew = null;
		Inline = null;
		Root = null;
		Parsers = null;
		Context = null;
		Document = null;
		DebugLog = null;
		PreciseSourceLocation = false;
		TrackTrivia = false;
		LineIndex = 0;
		previousSliceOffset = 0;
		previousLineIndexForSliceOffset = 0;
		LiteralInlineParser.PostMatch = null;
		lineOffsets.Clear();
		Array.Clear(ParserStates, 0, ParserStates.Length);
	}

	internal static InlineProcessor Rent(MarkdownDocument document, InlineParserList parsers, bool preciseSourcelocation, MarkdownParserContext? context, bool trackTrivia)
	{
		InlineProcessor inlineProcessor = _cache.Get();
		inlineProcessor.Setup(document, parsers, preciseSourcelocation, context, trackTrivia);
		return inlineProcessor;
	}

	internal static void Release(InlineProcessor processor)
	{
		_cache.Release(processor);
	}
}

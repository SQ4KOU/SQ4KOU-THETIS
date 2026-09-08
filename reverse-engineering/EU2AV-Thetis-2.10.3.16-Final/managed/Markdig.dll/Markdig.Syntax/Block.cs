using System;
using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax;

public abstract class Block : MarkdownObject, IBlock, IMarkdownObject
{
	private sealed class BlockTriviaProperties
	{
		public object? DerivedTriviaSlot;

		public ProcessInlineDelegate? ProcessInlinesBegin;

		public ProcessInlineDelegate? ProcessInlinesEnd;

		public StringSlice TriviaBefore;

		public StringSlice TriviaAfter;

		public List<StringSlice>? LinesBefore;

		public List<StringSlice>? LinesAfter;
	}

	private BlockTriviaProperties? _trivia => GetTrivia<BlockTriviaProperties>();

	private BlockTriviaProperties Trivia => GetOrSetTrivia<BlockTriviaProperties>();

	public ContainerBlock? Parent { get; internal set; }

	public BlockParser? Parser { get; }

	internal bool IsLeafBlock { get; private protected set; }

	internal bool IsParagraphBlock { get; private protected set; }

	public bool IsOpen { get; set; }

	public bool IsBreakable { get; set; }

	public NewLine NewLine { get; set; }

	public bool RemoveAfterProcessInlines { get; set; }

	public StringSlice TriviaBefore
	{
		get
		{
			return _trivia?.TriviaBefore ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaBefore = value;
		}
	}

	public StringSlice TriviaAfter
	{
		get
		{
			return _trivia?.TriviaAfter ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaAfter = value;
		}
	}

	public List<StringSlice>? LinesBefore
	{
		get
		{
			return _trivia?.LinesBefore;
		}
		set
		{
			Trivia.LinesBefore = value;
		}
	}

	public List<StringSlice>? LinesAfter
	{
		get
		{
			return _trivia?.LinesAfter;
		}
		set
		{
			Trivia.LinesAfter = value;
		}
	}

	public event ProcessInlineDelegate? ProcessInlinesBegin
	{
		add
		{
			BlockTriviaProperties trivia = Trivia;
			trivia.ProcessInlinesBegin = (ProcessInlineDelegate)Delegate.Combine(trivia.ProcessInlinesBegin, value);
		}
		remove
		{
			BlockTriviaProperties? trivia = _trivia;
			if (trivia != null)
			{
				trivia.ProcessInlinesBegin = (ProcessInlineDelegate)Delegate.Remove(trivia.ProcessInlinesBegin, value);
			}
		}
	}

	public event ProcessInlineDelegate? ProcessInlinesEnd
	{
		add
		{
			BlockTriviaProperties trivia = Trivia;
			trivia.ProcessInlinesEnd = (ProcessInlineDelegate)Delegate.Combine(trivia.ProcessInlinesEnd, value);
		}
		remove
		{
			BlockTriviaProperties? trivia = _trivia;
			if (trivia != null)
			{
				trivia.ProcessInlinesEnd = (ProcessInlineDelegate)Delegate.Remove(trivia.ProcessInlinesEnd, value);
			}
		}
	}

	protected Block(BlockParser? parser)
	{
		Parser = parser;
		IsOpen = true;
		IsBreakable = true;
		SetTypeKind(isInline: false, isContainer: false);
	}

	internal void OnProcessInlinesBegin(InlineProcessor state)
	{
		BlockTriviaProperties trivia = _trivia;
		if (trivia != null)
		{
			trivia.ProcessInlinesBegin?.Invoke(state, null);
			_trivia.ProcessInlinesBegin = null;
		}
	}

	internal void OnProcessInlinesEnd(InlineProcessor state)
	{
		BlockTriviaProperties trivia = _trivia;
		if (trivia != null)
		{
			trivia.ProcessInlinesEnd?.Invoke(state, null);
			_trivia.ProcessInlinesEnd = null;
		}
	}

	public void UpdateSpanToInclude(SourceSpan span)
	{
		if (span.IsEmpty)
		{
			return;
		}
		int num = 0;
		Block block = this;
		while (block != null)
		{
			if (block.Span.IsEmpty)
			{
				block.Span = span;
			}
			else
			{
				if (span.Start < block.Span.Start)
				{
					block.Span.Start = span.Start;
				}
				if (span.End > block.Span.End)
				{
					block.Span.End = span.End;
				}
			}
			block = block.Parent;
			num++;
		}
		ThrowHelper.CheckDepthLimit(num, useLargeLimit: true);
	}

	public void UpdateSpanEnd(int spanEnd)
	{
		int num = 0;
		Block block = this;
		while (block != null)
		{
			if (spanEnd > block.Span.End)
			{
				block.Span.End = spanEnd;
			}
			block = block.Parent;
			num++;
		}
		ThrowHelper.CheckDepthLimit(num, useLargeLimit: true);
	}

	public void Remove()
	{
		Parent?.Remove(this);
	}

	public void ReplaceBy(Block replacement, bool moveChildren = true)
	{
		if (replacement == null)
		{
			ThrowHelper.ArgumentNullException("replacement");
		}
		if (replacement.Parent != null)
		{
			ThrowHelper.ArgumentException("Cannot replace with a block that is already attached to another container (replacement.Parent != null)", "replacement");
		}
		ContainerBlock? parent = Parent;
		if (parent == null)
		{
			ThrowHelper.InvalidOperationException("Cannot replace a block that has no parent");
		}
		int num = parent.IndexOf(this);
		if (num < 0)
		{
			ThrowHelper.InvalidOperationException("Cannot replace a block that is not attached to its parent container");
		}
		parent[num] = replacement;
		if (moveChildren && this is ContainerBlock containerBlock && replacement is ContainerBlock destination)
		{
			containerBlock.TransferChildrenTo(destination);
		}
	}

	internal static Block FindRootMostContainerParent(Block block)
	{
		while (true)
		{
			Block parent = block.Parent;
			if (parent == null || !parent.IsContainerBlock || parent is MarkdownDocument)
			{
				break;
			}
			block = parent;
		}
		return block;
	}

	private protected T? TryGetDerivedTrivia<T>() where T : class
	{
		return _trivia?.DerivedTriviaSlot as T;
	}

	private protected T GetOrSetDerivedTrivia<T>() where T : new()
	{
		BlockTriviaProperties trivia = Trivia;
		return (T)(trivia.DerivedTriviaSlot ?? (trivia.DerivedTriviaSlot = new T()));
	}
}

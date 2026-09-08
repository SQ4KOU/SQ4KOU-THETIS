using System;
using System.Diagnostics;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace Markdig.Syntax;

[DebuggerDisplay("{GetType().Name} Line: {Line}, {Lines}")]
public abstract class LeafBlock : Block
{
	private ContainerInline? inline;

	public StringLineGroup Lines;

	public ContainerInline? Inline
	{
		get
		{
			return inline;
		}
		set
		{
			if (value != null)
			{
				if (value.Parent != null)
				{
					ThrowHelper.ArgumentException("Cannot add this inline as it as already attached to another container (inline.Parent != null)");
				}
				if (value.ParentBlock != null)
				{
					ThrowHelper.ArgumentException("Cannot add this inline as it as already attached to another container (inline.ParentBlock != null)");
				}
				value.ParentBlock = this;
			}
			if (inline != null)
			{
				inline.ParentBlock = null;
			}
			inline = value;
		}
	}

	public bool ProcessInlines { get; set; }

	protected LeafBlock(BlockParser? parser)
		: base(parser)
	{
		base.IsLeafBlock = true;
	}

	public void AppendLine(ref StringSlice slice, int column, int line, int sourceLinePosition, bool trackTrivia)
	{
		if (Lines.Lines == null)
		{
			Lines = new StringLineGroup(4, ProcessInlines);
		}
		StringLine line2 = new StringLine(ref slice, line, column, sourceLinePosition, slice.NewLine);
		if (slice.CurrentChar == '\t' && CharHelper.IsAcrossTab(column) && !trackTrivia)
		{
			Span<char> initialBuffer = stackalloc char[64];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			valueStringBuilder.Append(' ', CharHelper.AddTab(column) - column);
			valueStringBuilder.Append(slice.AsSpan().Slice(1));
			line2.Slice = new StringSlice(valueStringBuilder.ToString());
		}
		Lines.Add(ref line2);
		base.NewLine = slice.NewLine;
	}
}

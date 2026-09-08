using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace Markdig.Syntax;

[DebuggerDisplay("{GetType().Name} Count = {Count}")]
public abstract class ContainerBlock : Block, IList<Block>, ICollection<Block>, IEnumerable<Block>, IEnumerable, IReadOnlyList<Block>, IReadOnlyCollection<Block>
{
	public struct Enumerator : IEnumerator<Block>, IDisposable, IEnumerator
	{
		private readonly ContainerBlock block;

		private int index;

		private Block? current;

		public Block Current => current;

		object IEnumerator.Current => Current;

		internal Enumerator(ContainerBlock block)
		{
			this.block = block;
			index = 0;
			current = null;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (index < block.Count)
			{
				current = block[index];
				index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			index = block.Count + 1;
			current = null;
			return false;
		}

		void IEnumerator.Reset()
		{
			index = 0;
			current = null;
		}
	}

	private sealed class BlockComparisonWrapper(Comparison<Block> comparison) : IComparer<BlockWrapper>
	{
		private readonly Comparison<Block> _comparison = comparison;

		public int Compare(BlockWrapper x, BlockWrapper y)
		{
			return _comparison(x.Block, y.Block);
		}
	}

	private sealed class BlockComparerWrapper(IComparer<Block> comparer) : IComparer<BlockWrapper>
	{
		private readonly IComparer<Block> _comparer = comparer;

		public int Compare(BlockWrapper x, BlockWrapper y)
		{
			return _comparer.Compare(x.Block, y.Block);
		}
	}

	private BlockWrapper[] _children;

	public Block? LastChild
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			BlockWrapper[] children = _children;
			int num = Count - 1;
			if ((uint)num < (uint)children.Length)
			{
				return children[num].Block;
			}
			return null;
		}
	}

	public int Count { get; private set; }

	public bool IsReadOnly => false;

	public Block this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			BlockWrapper[] children = _children;
			if ((uint)index >= (uint)children.Length || index >= Count)
			{
				ThrowHelper.ThrowIndexOutOfRangeException();
				return null;
			}
			return children[index].Block;
		}
		set
		{
			if ((uint)index >= (uint)Count)
			{
				ThrowHelper.ThrowIndexOutOfRangeException();
			}
			if (value == null)
			{
				ThrowHelper.ArgumentNullException_item();
			}
			if (value.Parent != null)
			{
				ThrowHelper.ArgumentException("Cannot add this block as it as already attached to another container (block.Parent != null)");
			}
			Block block = _children[index].Block;
			if (block != null)
			{
				block.Parent = null;
			}
			value.Parent = this;
			_children[index] = new BlockWrapper(value);
		}
	}

	protected ContainerBlock(BlockParser? parser)
		: base(parser)
	{
		_children = Array.Empty<BlockWrapper>();
		SetTypeKind(isInline: false, isContainer: true);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<Block> IEnumerable<Block>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(Block item)
	{
		if (item == null)
		{
			ThrowHelper.ArgumentNullException_item();
		}
		if (item.Parent != null)
		{
			ThrowHelper.ArgumentException("Cannot add this block as it as already attached to another container (block.Parent != null)");
		}
		if (Count == _children.Length)
		{
			Grow();
		}
		_children[Count] = new BlockWrapper(item);
		Count++;
		item.Parent = this;
		UpdateSpanEnd(item.Span.End);
	}

	private void Grow()
	{
		if (_children.Length == 0)
		{
			_children = new BlockWrapper[4];
			return;
		}
		BlockWrapper[] array = new BlockWrapper[_children.Length * 2];
		Array.Copy(_children, 0, array, 0, Count);
		_children = array;
	}

	public void Clear()
	{
		BlockWrapper[] children = _children;
		for (int i = 0; i < Count && i < children.Length; i++)
		{
			children[i].Block.Parent = null;
			children[i] = default(BlockWrapper);
		}
		Count = 0;
	}

	public void TransferChildrenTo(ContainerBlock destination)
	{
		if (destination == null)
		{
			ThrowHelper.ArgumentNullException("destination");
		}
		if (this != destination)
		{
			BlockWrapper[] children = _children;
			int count = Count;
			for (int i = 0; i < count && i < children.Length; i++)
			{
				Block block = children[i].Block;
				block.Parent = null;
				children[i] = default(BlockWrapper);
				destination.Add(block);
			}
			Count = 0;
		}
	}

	public bool Contains(Block item)
	{
		return IndexOf(item) >= 0;
	}

	public void CopyTo(Block[] array, int arrayIndex)
	{
		BlockWrapper[] children = _children;
		for (int i = 0; i < Count && i < children.Length; i++)
		{
			array[arrayIndex + i] = children[i].Block;
		}
	}

	public bool Remove(Block item)
	{
		int num = IndexOf(item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public int IndexOf(Block item)
	{
		if (item == null)
		{
			ThrowHelper.ArgumentNullException_item();
		}
		BlockWrapper[] children = _children;
		for (int i = 0; i < Count && i < children.Length; i++)
		{
			if (children[i].Block == item)
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, Block item)
	{
		if (item == null)
		{
			ThrowHelper.ArgumentNullException_item();
		}
		if (item.Parent != null)
		{
			ThrowHelper.ArgumentException("Cannot add this block as it as already attached to another container (block.Parent != null)");
		}
		if ((uint)index > (uint)Count)
		{
			ThrowHelper.ArgumentOutOfRangeException_index();
		}
		if (Count == _children.Length)
		{
			Grow();
		}
		if (index < Count)
		{
			Array.Copy(_children, index, _children, index + 1, Count - index);
		}
		_children[index] = new BlockWrapper(item);
		Count++;
		item.Parent = this;
	}

	public void RemoveAt(int index)
	{
		if ((uint)index >= (uint)Count)
		{
			ThrowHelper.ArgumentOutOfRangeException_index();
		}
		Count--;
		_children[index].Block.Parent = null;
		if (index < Count)
		{
			Array.Copy(_children, index + 1, _children, index, Count - index);
		}
		_children[Count] = default(BlockWrapper);
	}

	public bool HasValidSpan(bool recursive = false)
	{
		BlockWrapper[] children = _children;
		for (int i = 0; i < Count && i < children.Length; i++)
		{
			Block block = children[i].Block;
			if (!ContainsSpan(in Span, in block.Span))
			{
				return false;
			}
			if (!recursive)
			{
				continue;
			}
			if (block is ContainerBlock containerBlock)
			{
				if (!containerBlock.HasValidSpan(recursive: true))
				{
					return false;
				}
			}
			else
			{
				if (!(block is LeafBlock leafBlock))
				{
					continue;
				}
				ContainerInline inline = leafBlock.Inline;
				if (inline != null)
				{
					if (!ContainsSpan(in leafBlock.Span, in inline.Span))
					{
						return false;
					}
					if (!inline.HasValidSpan(recursive: true))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool UpdateSpanFromChildren(bool recursive = false, bool preserveSelfSpan = true)
	{
		SourceSpan destinationSpan = SourceSpan.Empty;
		bool hasDestinationSpan = false;
		if (preserveSelfSpan && !Span.IsEmpty)
		{
			destinationSpan = Span;
			hasDestinationSpan = true;
		}
		BlockWrapper[] children = _children;
		for (int i = 0; i < Count && i < children.Length; i++)
		{
			Block block = children[i].Block;
			if (recursive)
			{
				if (block is ContainerBlock containerBlock)
				{
					containerBlock.UpdateSpanFromChildren(recursive: true, preserveSelfSpan);
				}
				else if (block is LeafBlock leafBlock)
				{
					ContainerInline inline = leafBlock.Inline;
					if (inline != null)
					{
						inline.UpdateSpanFromChildren(recursive: true, preserveSelfSpan);
						if (!ContainsSpan(in leafBlock.Span, in inline.Span))
						{
							if (preserveSelfSpan && !leafBlock.Span.IsEmpty)
							{
								leafBlock.UpdateSpanToInclude(inline.Span);
							}
							else
							{
								leafBlock.Span = inline.Span;
							}
						}
					}
				}
			}
			AppendSpan(ref destinationSpan, ref hasDestinationSpan, in block.Span);
		}
		if (!hasDestinationSpan)
		{
			destinationSpan = SourceSpan.Empty;
		}
		if (destinationSpan == Span)
		{
			return false;
		}
		Span = destinationSpan;
		return true;
	}

	public void Sort(IComparer<Block> comparer)
	{
		if (comparer == null)
		{
			ThrowHelper.ArgumentNullException("comparer");
		}
		Array.Sort(_children, 0, Count, new BlockComparerWrapper(comparer));
	}

	public void Sort(Comparison<Block> comparison)
	{
		if (comparison == null)
		{
			ThrowHelper.ArgumentNullException("comparison");
		}
		Array.Sort(_children, 0, Count, new BlockComparisonWrapper(comparison));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ContainsSpan(in SourceSpan containerSpan, in SourceSpan childSpan)
	{
		if (!childSpan.IsEmpty)
		{
			if (!containerSpan.IsEmpty && childSpan.Start >= containerSpan.Start)
			{
				return childSpan.End <= containerSpan.End;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AppendSpan(ref SourceSpan destinationSpan, ref bool hasDestinationSpan, in SourceSpan spanToAppend)
	{
		if (spanToAppend.IsEmpty)
		{
			return;
		}
		if (!hasDestinationSpan)
		{
			destinationSpan = spanToAppend;
			hasDestinationSpan = true;
			return;
		}
		if (spanToAppend.Start < destinationSpan.Start)
		{
			destinationSpan.Start = spanToAppend.Start;
		}
		if (spanToAppend.End > destinationSpan.End)
		{
			destinationSpan.End = spanToAppend.End;
		}
	}
}

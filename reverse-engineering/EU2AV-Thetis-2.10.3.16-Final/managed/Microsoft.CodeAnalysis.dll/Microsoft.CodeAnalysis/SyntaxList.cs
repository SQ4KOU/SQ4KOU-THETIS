using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

public static class SyntaxList
{
	public static SyntaxList<TNode> Create<TNode>(ReadOnlySpan<TNode> nodes) where TNode : SyntaxNode
	{
		if (nodes.Length == 0)
		{
			return default(SyntaxList<TNode>);
		}
		return new SyntaxList<TNode>(createGreenNode(nodes).CreateRed());
		static GreenNode createGreenNode(ReadOnlySpan<TNode> readOnlySpan)
		{
			switch (readOnlySpan.Length)
			{
			case 1:
			{
				TNode val = readOnlySpan[0];
				return val.Green;
			}
			case 2:
			{
				TNode val = readOnlySpan[0];
				GreenNode green = val.Green;
				val = readOnlySpan[1];
				return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(green, val.Green);
			}
			case 3:
			{
				TNode val = readOnlySpan[0];
				GreenNode green2 = val.Green;
				val = readOnlySpan[1];
				GreenNode green3 = val.Green;
				val = readOnlySpan[2];
				return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(green2, green3, val.Green);
			}
			default:
			{
				ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[readOnlySpan.Length];
				int i = 0;
				for (int length = readOnlySpan.Length; i < length; i++)
				{
					ref ArrayElement<GreenNode> reference = ref array[i];
					TNode val = readOnlySpan[i];
					reference.Value = val.Green;
				}
				return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(array);
			}
			}
		}
	}
}
[CollectionBuilder(typeof(SyntaxList), "Create")]
public readonly struct SyntaxList<TNode> : IReadOnlyList<TNode>, IEnumerable<TNode>, IEnumerable, IReadOnlyCollection<TNode>, IEquatable<SyntaxList<TNode>> where TNode : SyntaxNode
{
	public struct Enumerator
	{
		private readonly SyntaxList<TNode> _list;

		private int _index;

		public TNode Current => (TNode)_list.ItemInternal(_index);

		internal Enumerator(SyntaxList<TNode> list)
		{
			_list = list;
			_index = -1;
		}

		public bool MoveNext()
		{
			int num = _index + 1;
			if (num < _list.Count)
			{
				_index = num;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_index = -1;
		}

		public override bool Equals(object? obj)
		{
			throw new NotSupportedException();
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException();
		}
	}

	private class EnumeratorImpl : IEnumerator<TNode>, IEnumerator, IDisposable
	{
		private Enumerator _e;

		public TNode Current => _e.Current;

		object IEnumerator.Current => _e.Current;

		internal EnumeratorImpl(in SyntaxList<TNode> list)
		{
			_e = new Enumerator(list);
		}

		public bool MoveNext()
		{
			return _e.MoveNext();
		}

		void IDisposable.Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			_e.Reset();
		}
	}

	private readonly SyntaxNode? _node;

	internal SyntaxNode? Node => _node;

	public int Count
	{
		get
		{
			if (_node != null)
			{
				if (!_node.IsList)
				{
					return 1;
				}
				return _node.SlotCount;
			}
			return 0;
		}
	}

	public TNode this[int index]
	{
		get
		{
			if (_node != null)
			{
				if (_node.IsList)
				{
					if ((uint)index < (uint)_node.SlotCount)
					{
						return (TNode)_node.GetNodeSlot(index);
					}
				}
				else if (index == 0)
				{
					return (TNode)_node;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public TextSpan FullSpan
	{
		get
		{
			if (Count == 0)
			{
				return default(TextSpan);
			}
			return TextSpan.FromBounds(this[0].FullSpan.Start, this[Count - 1].FullSpan.End);
		}
	}

	public TextSpan Span
	{
		get
		{
			if (Count == 0)
			{
				return default(TextSpan);
			}
			return TextSpan.FromBounds(this[0].Span.Start, this[Count - 1].Span.End);
		}
	}

	private TNode[] Nodes => this.ToArray();

	internal SyntaxList(SyntaxNode? node)
	{
		_node = node;
	}

	public SyntaxList(TNode? node)
		: this((SyntaxNode?)node)
	{
	}

	public SyntaxList(IEnumerable<TNode>? nodes)
		: this(CreateNode(nodes))
	{
	}

	private static SyntaxNode? CreateNode(IEnumerable<TNode>? nodes)
	{
		if (nodes == null)
		{
			return null;
		}
		Microsoft.CodeAnalysis.Syntax.SyntaxListBuilder<TNode> syntaxListBuilder = ((nodes is ICollection<TNode> collection) ? new Microsoft.CodeAnalysis.Syntax.SyntaxListBuilder<TNode>(collection.Count) : Microsoft.CodeAnalysis.Syntax.SyntaxListBuilder<TNode>.Create());
		foreach (TNode node in nodes)
		{
			syntaxListBuilder.Add(node);
		}
		return syntaxListBuilder.ToList().Node;
	}

	internal SyntaxNode? ItemInternal(int index)
	{
		SyntaxNode? node = _node;
		if (node != null && node.IsList)
		{
			return _node.GetNodeSlot(index);
		}
		return _node;
	}

	public override string ToString()
	{
		if (_node == null)
		{
			return string.Empty;
		}
		return _node.ToString();
	}

	public string ToFullString()
	{
		if (_node == null)
		{
			return string.Empty;
		}
		return _node.ToFullString();
	}

	public SyntaxList<TNode> Add(TNode node)
	{
		return Insert(Count, node);
	}

	public SyntaxList<TNode> AddRange(IEnumerable<TNode> nodes)
	{
		return InsertRange(Count, nodes);
	}

	public SyntaxList<TNode> Insert(int index, TNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return InsertRange(index, new TNode[1] { node });
	}

	public SyntaxList<TNode> InsertRange(int index, IEnumerable<TNode> nodes)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		List<TNode> list = this.ToList();
		list.InsertRange(index, nodes);
		if (list.Count == 0)
		{
			return this;
		}
		return CreateList(list);
	}

	public SyntaxList<TNode> RemoveAt(int index)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return Remove(this[index]);
	}

	public SyntaxList<TNode> Remove(TNode node)
	{
		return CreateList(this.Where((TNode x) => x != node).ToList());
	}

	public SyntaxList<TNode> Replace(TNode nodeInList, TNode newNode)
	{
		return ReplaceRange(nodeInList, new TNode[1] { newNode });
	}

	public SyntaxList<TNode> ReplaceRange(TNode nodeInList, IEnumerable<TNode> newNodes)
	{
		if (nodeInList == null)
		{
			throw new ArgumentNullException("nodeInList");
		}
		if (newNodes == null)
		{
			throw new ArgumentNullException("newNodes");
		}
		int num = IndexOf(nodeInList);
		if (num >= 0 && num < Count)
		{
			List<TNode> list = this.ToList();
			list.RemoveAt(num);
			list.InsertRange(num, newNodes);
			return CreateList(list);
		}
		throw new ArgumentException("nodeInList");
	}

	private static SyntaxList<TNode> CreateList(List<TNode> items)
	{
		if (items.Count == 0)
		{
			return default(SyntaxList<TNode>);
		}
		return new SyntaxList<TNode>(GreenNode.CreateList(items, (TNode n) => n.Green).CreateRed());
	}

	public TNode First()
	{
		return this[0];
	}

	public TNode? FirstOrDefault()
	{
		if (Any())
		{
			return this[0];
		}
		return null;
	}

	public TNode Last()
	{
		return this[Count - 1];
	}

	public TNode? LastOrDefault()
	{
		if (Any())
		{
			return this[Count - 1];
		}
		return null;
	}

	public bool Any()
	{
		return _node != null;
	}

	internal bool All(Func<TNode, bool> predicate)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TNode current = enumerator.Current;
			if (!predicate(current))
			{
				return false;
			}
		}
		return true;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
	{
		if (Any())
		{
			return new EnumeratorImpl(this);
		}
		return SpecializedCollections.EmptyEnumerator<TNode>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Any())
		{
			return new EnumeratorImpl(this);
		}
		return SpecializedCollections.EmptyEnumerator<TNode>();
	}

	public static bool operator ==(SyntaxList<TNode> left, SyntaxList<TNode> right)
	{
		return left._node == right._node;
	}

	public static bool operator !=(SyntaxList<TNode> left, SyntaxList<TNode> right)
	{
		return left._node != right._node;
	}

	public bool Equals(SyntaxList<TNode> other)
	{
		return _node == other._node;
	}

	public override bool Equals(object? obj)
	{
		if (obj is SyntaxList<TNode>)
		{
			return Equals((SyntaxList<TNode>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _node?.GetHashCode() ?? 0;
	}

	[Obsolete("This method is preserved for binary compatibility only. Use explicit cast instead.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SyntaxList<TNode> op_Implicit(SyntaxList<SyntaxNode> nodes)
	{
		return new SyntaxList<TNode>(nodes._node);
	}

	public static implicit operator SyntaxList<SyntaxNode>(SyntaxList<TNode> nodes)
	{
		return new SyntaxList<SyntaxNode>(nodes.Node);
	}

	public static explicit operator SyntaxList<TNode>(SyntaxList<SyntaxNode> nodes)
	{
		return new SyntaxList<TNode>(nodes._node);
	}

	public int IndexOf(TNode node)
	{
		int num = 0;
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (object.Equals(enumerator.Current, node))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public int IndexOf(Func<TNode, bool> predicate)
	{
		int num = 0;
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TNode current = enumerator.Current;
			if (predicate(current))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	internal int IndexOf(int rawKind)
	{
		int num = 0;
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.RawKind == rawKind)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public int LastIndexOf(TNode node)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			if (object.Equals(this[num], node))
			{
				return num;
			}
		}
		return -1;
	}

	public int LastIndexOf(Func<TNode, bool> predicate)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			if (predicate(this[num]))
			{
				return num;
			}
		}
		return -1;
	}
}

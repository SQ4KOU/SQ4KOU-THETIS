using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class GreenNode
{
	[Flags]
	internal enum NodeFlags : ushort
	{
		None = 0,
		IsNotMissing = 1,
		HasAnnotationsDirectly = 2,
		FactoryContextIsInAsync = 4,
		FactoryContextIsInQuery = 8,
		FactoryContextIsInIterator = FactoryContextIsInQuery,
		FactoryContextIsInFieldKeywordContext = 0x10,
		ContainsAnnotations = 0x20,
		ContainsAttributes = 0x40,
		ContainsDiagnostics = 0x80,
		ContainsDirectives = 0x100,
		ContainsSkippedText = 0x200,
		ContainsStructuredTrivia = 0x400,
		InheritMask = IsNotMissing | ContainsAnnotations | ContainsAttributes | ContainsDiagnostics | ContainsDirectives | ContainsSkippedText | ContainsStructuredTrivia
	}

	[NonCopyable]
	public ref struct NodeEnumerable(GreenNode node)
	{
		[NonCopyable]
		public ref struct Enumerator
		{
			private readonly ArrayBuilder<Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator> _stack;

			private bool _started;

			private GreenNode _current;

			public readonly GreenNode Current => _current;

			public Enumerator(GreenNode node)
			{
				_started = false;
				_current = node;
				_stack = ArrayBuilder<Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator>.GetInstance();
				_stack.Push(node.ChildNodesAndTokens().GetEnumerator());
			}

			public readonly void Dispose()
			{
				_stack.Free();
			}

			public bool MoveNext()
			{
				if (!_started)
				{
					_started = true;
					return true;
				}
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator result;
				while (_stack.TryPop(out result))
				{
					if (result.MoveNext())
					{
						_current = result.Current;
						_stack.Push(result);
						if (!_current.IsToken)
						{
							_stack.Push(_current.ChildNodesAndTokens().GetEnumerator());
						}
						return true;
					}
				}
				return false;
			}
		}

		private readonly GreenNode _node = node;

		public readonly Enumerator GetEnumerator()
		{
			return new Enumerator(_node);
		}
	}

	private struct NodeFlagsAndSlotCount
	{
		private const ushort SlotCountMask = 61440;

		private const ushort NodeFlagsMask = 4095;

		private const int SlotCountShift = 12;

		private ushort _data;

		public byte SmallSlotCount
		{
			readonly get
			{
				return (byte)(_data >> 12);
			}
			set
			{
				if (value > 15)
				{
					value = 15;
				}
				_data = (ushort)((_data & 0xFFF) | (value << 12));
			}
		}

		public NodeFlags NodeFlags
		{
			readonly get
			{
				return (NodeFlags)(_data & 0xFFF);
			}
			set
			{
				_data = (ushort)((uint)(_data & 0xF000) | (uint)value);
			}
		}
	}

	internal const int ListKind = 1;

	protected const int SlotCountTooLarge = 15;

	private readonly ushort _kind;

	private NodeFlagsAndSlotCount _nodeFlagsAndSlotCount;

	private int _fullWidth;

	private static readonly ConditionalWeakTable<GreenNode, DiagnosticInfo[]> s_diagnosticsTable = new ConditionalWeakTable<GreenNode, DiagnosticInfo[]>();

	private static readonly ConditionalWeakTable<GreenNode, SyntaxAnnotation[]> s_annotationsTable = new ConditionalWeakTable<GreenNode, SyntaxAnnotation[]>();

	private static readonly DiagnosticInfo[] s_noDiagnostics = Array.Empty<DiagnosticInfo>();

	private static readonly SyntaxAnnotation[] s_noAnnotations = Array.Empty<SyntaxAnnotation>();

	private static readonly IEnumerable<SyntaxAnnotation> s_noAnnotationsEnumerable = SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();

	internal const int MaxCachedChildNum = 3;

	public abstract string Language { get; }

	public int RawKind => _kind;

	public bool IsList => RawKind == 1;

	public abstract string KindText { get; }

	public virtual bool IsStructuredTrivia => false;

	public virtual bool IsDirective => false;

	public virtual bool IsToken => false;

	public virtual bool IsTrivia => false;

	public virtual bool IsSkippedTokensTrivia => false;

	public virtual bool IsDocumentationCommentTrivia => false;

	public int SlotCount
	{
		get
		{
			byte smallSlotCount = _nodeFlagsAndSlotCount.SmallSlotCount;
			if (smallSlotCount != 15)
			{
				return smallSlotCount;
			}
			return GetSlotCount();
		}
		protected set
		{
			_nodeFlagsAndSlotCount.SmallSlotCount = (byte)value;
		}
	}

	internal NodeFlags Flags => _nodeFlagsAndSlotCount.NodeFlags;

	internal bool IsMissing => (Flags & NodeFlags.IsNotMissing) == 0;

	internal bool ParsedInAsync => (Flags & NodeFlags.FactoryContextIsInAsync) != 0;

	internal bool ParsedInQuery => (Flags & NodeFlags.FactoryContextIsInQuery) != 0;

	internal bool ParsedInIterator => (Flags & NodeFlags.FactoryContextIsInQuery) != 0;

	internal bool ParsedInFieldKeywordContext => (Flags & NodeFlags.FactoryContextIsInFieldKeywordContext) != 0;

	public bool ContainsSkippedText => (Flags & NodeFlags.ContainsSkippedText) != 0;

	public bool ContainsStructuredTrivia => (Flags & NodeFlags.ContainsStructuredTrivia) != 0;

	public bool ContainsDirectives => (Flags & NodeFlags.ContainsDirectives) != 0;

	public bool ContainsAttributes => (Flags & NodeFlags.ContainsAttributes) != 0;

	public bool ContainsDiagnostics => (Flags & NodeFlags.ContainsDiagnostics) != 0;

	public bool ContainsAnnotations => (Flags & NodeFlags.ContainsAnnotations) != 0;

	public bool HasAnnotationsDirectly => (Flags & NodeFlags.HasAnnotationsDirectly) != 0;

	public int FullWidth
	{
		get
		{
			return _fullWidth;
		}
		protected set
		{
			_fullWidth = value;
		}
	}

	public virtual int Width => _fullWidth - GetLeadingTriviaWidth() - GetTrailingTriviaWidth();

	public bool HasLeadingTrivia => GetLeadingTriviaWidth() != 0;

	public bool HasTrailingTrivia => GetTrailingTriviaWidth() != 0;

	public virtual int RawContextualKind => RawKind;

	internal bool IsCacheable
	{
		get
		{
			if ((Flags & NodeFlags.InheritMask) == NodeFlags.IsNotMissing)
			{
				return SlotCount <= 3;
			}
			return false;
		}
	}

	private string GetDebuggerDisplay()
	{
		return GetType().Name + " " + KindText + " " + ToString();
	}

	protected GreenNode(ushort kind)
	{
		_kind = kind;
	}

	protected GreenNode(ushort kind, int fullWidth)
	{
		_kind = kind;
		_fullWidth = fullWidth;
	}

	protected GreenNode(ushort kind, DiagnosticInfo[]? diagnostics, int fullWidth)
	{
		_kind = kind;
		_fullWidth = fullWidth;
		if (diagnostics != null && diagnostics.Length != 0)
		{
			SetFlags(NodeFlags.ContainsDiagnostics);
			s_diagnosticsTable.Add(this, diagnostics);
		}
	}

	protected GreenNode(ushort kind, DiagnosticInfo[]? diagnostics)
	{
		_kind = kind;
		if (diagnostics != null && diagnostics.Length != 0)
		{
			SetFlags(NodeFlags.ContainsDiagnostics);
			s_diagnosticsTable.Add(this, diagnostics);
		}
	}

	protected GreenNode(ushort kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: this(kind, diagnostics)
	{
		if (annotations == null || annotations.Length == 0)
		{
			return;
		}
		for (int i = 0; i < annotations.Length; i++)
		{
			if (annotations[i] == null)
			{
				throw new ArgumentException("", "annotations");
			}
		}
		SetFlags(NodeFlags.HasAnnotationsDirectly | NodeFlags.ContainsAnnotations);
		s_annotationsTable.Add(this, annotations);
	}

	protected GreenNode(ushort kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, int fullWidth)
		: this(kind, diagnostics, fullWidth)
	{
		if (annotations == null || annotations.Length == 0)
		{
			return;
		}
		for (int i = 0; i < annotations.Length; i++)
		{
			if (annotations[i] == null)
			{
				throw new ArgumentException("", "annotations");
			}
		}
		SetFlags(NodeFlags.HasAnnotationsDirectly | NodeFlags.ContainsAnnotations);
		s_annotationsTable.Add(this, annotations);
	}

	protected void AdjustFlagsAndWidth(GreenNode node)
	{
		SetFlags(node.Flags & NodeFlags.InheritMask);
		_fullWidth += node._fullWidth;
	}

	internal abstract GreenNode? GetSlot(int index);

	internal GreenNode GetRequiredSlot(int index)
	{
		return GetSlot(index);
	}

	protected virtual int GetSlotCount()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Syntax/GreenNode.cs", 182);
	}

	public virtual int GetSlotOffset(int index)
	{
		int num = 0;
		for (int i = 0; i < index; i++)
		{
			GreenNode slot = GetSlot(i);
			if (slot != null)
			{
				num += slot.FullWidth;
			}
		}
		return num;
	}

	internal Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList ChildNodesAndTokens()
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList(this);
	}

	public NodeEnumerable EnumerateNodes()
	{
		return new NodeEnumerable(this);
	}

	public virtual int FindSlotIndexContainingOffset(int offset)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			GreenNode slot = GetSlot(num2);
			if (slot != null)
			{
				num += slot.FullWidth;
				if (offset < num)
				{
					break;
				}
			}
			num2++;
		}
		return num2;
	}

	internal void SetFlags(NodeFlags flags)
	{
		_nodeFlagsAndSlotCount.NodeFlags |= flags;
	}

	internal void ClearFlags(NodeFlags flags)
	{
		_nodeFlagsAndSlotCount.NodeFlags &= (NodeFlags)(ushort)(~(int)flags);
	}

	public virtual int GetLeadingTriviaWidth()
	{
		if (FullWidth == 0)
		{
			return 0;
		}
		return GetFirstTerminal().GetLeadingTriviaWidth();
	}

	public virtual int GetTrailingTriviaWidth()
	{
		if (FullWidth == 0)
		{
			return 0;
		}
		return GetLastTerminal().GetTrailingTriviaWidth();
	}

	public bool HasAnnotations(string annotationKind)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return false;
		}
		SyntaxAnnotation[] array = annotations;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Kind == annotationKind)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnnotations(IEnumerable<string> annotationKinds)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return false;
		}
		SyntaxAnnotation[] array = annotations;
		foreach (SyntaxAnnotation syntaxAnnotation in array)
		{
			if (annotationKinds.Contains<string>(syntaxAnnotation.Kind))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnnotation([NotNullWhen(true)] SyntaxAnnotation? annotation)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return false;
		}
		SyntaxAnnotation[] array = annotations;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == annotation)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(string annotationKind)
	{
		if (string.IsNullOrWhiteSpace(annotationKind))
		{
			throw new ArgumentNullException("annotationKind");
		}
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return s_noAnnotationsEnumerable;
		}
		return GetAnnotationsSlow(annotations, annotationKind);
	}

	private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, string annotationKind)
	{
		foreach (SyntaxAnnotation syntaxAnnotation in annotations)
		{
			if (syntaxAnnotation.Kind == annotationKind)
			{
				yield return syntaxAnnotation;
			}
		}
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<string> annotationKinds)
	{
		if (annotationKinds == null)
		{
			throw new ArgumentNullException("annotationKinds");
		}
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return s_noAnnotationsEnumerable;
		}
		return GetAnnotationsSlow(annotations, annotationKinds);
	}

	private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, IEnumerable<string> annotationKinds)
	{
		foreach (SyntaxAnnotation syntaxAnnotation in annotations)
		{
			if (annotationKinds.Contains<string>(syntaxAnnotation.Kind))
			{
				yield return syntaxAnnotation;
			}
		}
	}

	public SyntaxAnnotation[] GetAnnotations()
	{
		if (!HasAnnotationsDirectly)
		{
			return s_noAnnotations;
		}
		s_annotationsTable.TryGetValue(this, out SyntaxAnnotation[] value);
		return value;
	}

	internal abstract GreenNode SetAnnotations(SyntaxAnnotation[]? annotations);

	internal DiagnosticInfo[] GetDiagnostics()
	{
		if (ContainsDiagnostics && s_diagnosticsTable.TryGetValue(this, out DiagnosticInfo[] value))
		{
			return value;
		}
		return s_noDiagnostics;
	}

	internal abstract GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics);

	public virtual string ToFullString()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringWriter writer = new StringWriter(instance.Builder, CultureInfo.InvariantCulture);
		WriteTo(writer, leading: true, trailing: true);
		return instance.ToStringAndFree();
	}

	public override string ToString()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringWriter writer = new StringWriter(instance.Builder, CultureInfo.InvariantCulture);
		WriteTo(writer, leading: false, trailing: false);
		return instance.ToStringAndFree();
	}

	public void WriteTo(TextWriter writer)
	{
		WriteTo(writer, leading: true, trailing: true);
	}

	protected internal void WriteTo(TextWriter writer, bool leading, bool trailing)
	{
		ArrayBuilder<(GreenNode, bool, bool)> instance = ArrayBuilder<(GreenNode, bool, bool)>.GetInstance();
		instance.Push((this, leading, trailing));
		processStack(writer, instance);
		instance.Free();
		static void processStack(TextWriter writer2, ArrayBuilder<(GreenNode node, bool leading, bool trailing)> stack)
		{
			while (stack.Count > 0)
			{
				var (greenNode, flag, flag2) = stack.Pop();
				if (greenNode.IsToken)
				{
					greenNode.WriteTokenTo(writer2, flag, flag2);
				}
				else if (greenNode.IsTrivia)
				{
					greenNode.WriteTriviaTo(writer2);
				}
				else
				{
					int firstNonNullChildIndex = GetFirstNonNullChildIndex(greenNode);
					int lastNonNullChildIndex = GetLastNonNullChildIndex(greenNode);
					for (int num = lastNonNullChildIndex; num >= firstNonNullChildIndex; num--)
					{
						GreenNode slot = greenNode.GetSlot(num);
						if (slot != null)
						{
							bool flag3 = num == firstNonNullChildIndex;
							bool flag4 = num == lastNonNullChildIndex;
							stack.Push((slot, flag | !flag3, flag2 | !flag4));
						}
					}
				}
			}
		}
	}

	private static int GetFirstNonNullChildIndex(GreenNode node)
	{
		int slotCount = node.SlotCount;
		int i;
		for (i = 0; i < slotCount && node.GetSlot(i) == null; i++)
		{
		}
		return i;
	}

	private static int GetLastNonNullChildIndex(GreenNode node)
	{
		int num = node.SlotCount - 1;
		while (num >= 0 && node.GetSlot(num) == null)
		{
			num--;
		}
		return num;
	}

	protected virtual void WriteTriviaTo(TextWriter writer)
	{
		throw new NotImplementedException();
	}

	protected virtual void WriteTokenTo(TextWriter writer, bool leading, bool trailing)
	{
		throw new NotImplementedException();
	}

	public virtual object? GetValue()
	{
		return null;
	}

	public virtual string GetValueText()
	{
		return string.Empty;
	}

	public virtual GreenNode? GetLeadingTriviaCore()
	{
		return null;
	}

	public virtual GreenNode? GetTrailingTriviaCore()
	{
		return null;
	}

	public virtual GreenNode WithLeadingTrivia(GreenNode? trivia)
	{
		return this;
	}

	public virtual GreenNode WithTrailingTrivia(GreenNode? trivia)
	{
		return this;
	}

	internal GreenNode? GetFirstTerminal()
	{
		GreenNode greenNode = this;
		do
		{
			GreenNode greenNode2 = null;
			int i = 0;
			for (int slotCount = greenNode.SlotCount; i < slotCount; i++)
			{
				GreenNode slot = greenNode.GetSlot(i);
				if (slot != null)
				{
					greenNode2 = slot;
					break;
				}
			}
			greenNode = greenNode2;
		}
		while (greenNode?._nodeFlagsAndSlotCount.SmallSlotCount > 0);
		return greenNode;
	}

	internal GreenNode? GetLastTerminal()
	{
		GreenNode greenNode = this;
		do
		{
			GreenNode greenNode2 = null;
			for (int num = greenNode.SlotCount - 1; num >= 0; num--)
			{
				GreenNode slot = greenNode.GetSlot(num);
				if (slot != null)
				{
					greenNode2 = slot;
					break;
				}
			}
			greenNode = greenNode2;
		}
		while (greenNode?._nodeFlagsAndSlotCount.SmallSlotCount > 0);
		return greenNode;
	}

	internal GreenNode? GetLastNonmissingTerminal()
	{
		GreenNode greenNode = this;
		do
		{
			GreenNode greenNode2 = null;
			for (int num = greenNode.SlotCount - 1; num >= 0; num--)
			{
				GreenNode slot = greenNode.GetSlot(num);
				if (slot != null && !slot.IsMissing)
				{
					greenNode2 = slot;
					break;
				}
			}
			greenNode = greenNode2;
		}
		while (greenNode?._nodeFlagsAndSlotCount.SmallSlotCount > 0);
		return greenNode;
	}

	public virtual bool IsEquivalentTo([NotNullWhen(true)] GreenNode? other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return EquivalentToInternal(this, other);
	}

	private static bool EquivalentToInternal(GreenNode node1, GreenNode node2)
	{
		if (node1.RawKind != node2.RawKind)
		{
			if (node1.IsList && node1.SlotCount == 1)
			{
				node1 = node1.GetRequiredSlot(0);
			}
			if (node2.IsList && node2.SlotCount == 1)
			{
				node2 = node2.GetRequiredSlot(0);
			}
			if (node1.RawKind != node2.RawKind)
			{
				return false;
			}
		}
		if (node1._fullWidth != node2._fullWidth)
		{
			return false;
		}
		int slotCount = node1.SlotCount;
		if (slotCount != node2.SlotCount)
		{
			return false;
		}
		for (int i = 0; i < slotCount; i++)
		{
			GreenNode slot = node1.GetSlot(i);
			GreenNode slot2 = node2.GetSlot(i);
			if (slot != null && slot2 != null && !slot.IsEquivalentTo(slot2))
			{
				return false;
			}
		}
		return true;
	}

	public abstract SyntaxNode GetStructure(SyntaxTrivia parentTrivia);

	public abstract SyntaxToken CreateSeparator(SyntaxNode element);

	public abstract bool IsTriviaWithEndOfLine();

	public static GreenNode? CreateList<TFrom>(IEnumerable<TFrom>? enumerable, Func<TFrom, GreenNode> select)
	{
		if (enumerable != null)
		{
			if (!(enumerable is List<TFrom> list))
			{
				if (enumerable is IReadOnlyList<TFrom> list2)
				{
					return CreateList(list2, select);
				}
				return CreateList(enumerable.ToList(), select);
			}
			return CreateList(list, select);
		}
		return null;
	}

	public static GreenNode? CreateList<TFrom>(List<TFrom> list, Func<TFrom, GreenNode> select)
	{
		switch (list.Count)
		{
		case 0:
			return null;
		case 1:
			return select(list[0]);
		case 2:
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(select(list[0]), select(list[1]));
		case 3:
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(select(list[0]), select(list[1]), select(list[2]));
		default:
		{
			ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = select(list[i]);
			}
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(array);
		}
		}
	}

	public static GreenNode? CreateList<TFrom>(IReadOnlyList<TFrom> list, Func<TFrom, GreenNode> select)
	{
		switch (list.Count)
		{
		case 0:
			return null;
		case 1:
			return select(list[0]);
		case 2:
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(select(list[0]), select(list[1]));
		case 3:
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(select(list[0]), select(list[1]), select(list[2]));
		default:
		{
			ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = select(list[i]);
			}
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(array);
		}
		}
	}

	public SyntaxNode CreateRed()
	{
		return CreateRed(null, 0);
	}

	internal abstract SyntaxNode CreateRed(SyntaxNode? parent, int position);

	internal int GetCacheHash()
	{
		int num = (int)Flags ^ RawKind;
		int slotCount = SlotCount;
		for (int i = 0; i < slotCount; i++)
		{
			GreenNode slot = GetSlot(i);
			if (slot != null)
			{
				num = Hash.Combine(RuntimeHelpers.GetHashCode(slot), num);
			}
		}
		return num & 0x7FFFFFFF;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flags, GreenNode? child1)
	{
		if (RawKind == kind && Flags == flags && SlotCount == 1)
		{
			return GetSlot(0) == child1;
		}
		return false;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flags, GreenNode? child1, GreenNode? child2)
	{
		if (RawKind == kind && Flags == flags && SlotCount == 2 && GetSlot(0) == child1)
		{
			return GetSlot(1) == child2;
		}
		return false;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flags, GreenNode? child1, GreenNode? child2, GreenNode? child3)
	{
		if (RawKind == kind && Flags == flags && SlotCount == 3 && GetSlot(0) == child1 && GetSlot(1) == child2)
		{
			return GetSlot(2) == child3;
		}
		return false;
	}

	internal GreenNode AddError(DiagnosticInfo err)
	{
		DiagnosticInfo[] array;
		if (GetDiagnostics() == null)
		{
			array = new DiagnosticInfo[1] { err };
		}
		else
		{
			array = GetDiagnostics();
			int num = array.Length;
			Array.Resize(ref array, num + 1);
			array[num] = err;
		}
		return SetDiagnostics(array);
	}
}

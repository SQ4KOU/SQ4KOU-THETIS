using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Markdig.Helpers;

namespace Markdig.Syntax.Inlines;

public class ContainerInline : Inline, IEnumerable<Inline>, IEnumerable
{
	public struct Enumerator : IEnumerator<Inline>, IDisposable, IEnumerator
	{
		private readonly ContainerInline container;

		private Inline? currentChild;

		private Inline? nextChild;

		public Inline Current => currentChild;

		object IEnumerator.Current => Current;

		public Enumerator(ContainerInline container)
		{
			this = default(Enumerator);
			if (container == null)
			{
				ThrowHelper.ArgumentNullException("container");
			}
			this.container = container;
			currentChild = (nextChild = container.FirstChild);
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			currentChild = nextChild;
			if (currentChild != null)
			{
				nextChild = currentChild.NextSibling;
				return true;
			}
			nextChild = null;
			return false;
		}

		public void Reset()
		{
			currentChild = (nextChild = container.FirstChild);
		}
	}

	public LeafBlock? ParentBlock { get; internal set; }

	public Inline? FirstChild { get; private set; }

	public Inline? LastChild { get; private set; }

	public ContainerInline()
		: base(dummySkipTypeKind: true)
	{
		SetTypeKind(isInline: true, isContainer: true);
	}

	public void Clear()
	{
		for (Inline inline = LastChild; inline != null; inline = inline.PreviousSibling)
		{
			inline.Parent = null;
		}
		FirstChild = null;
		LastChild = null;
	}

	public virtual ContainerInline AppendChild(Inline child)
	{
		if (child == null)
		{
			ThrowHelper.ArgumentNullException("child");
		}
		if (child.Parent != null)
		{
			ThrowHelper.ArgumentException("Inline has already a parent", "child");
		}
		if (FirstChild == null)
		{
			FirstChild = child;
			LastChild = child;
			child.Parent = this;
		}
		else
		{
			LastChild.InsertAfter(child);
		}
		return this;
	}

	public bool ContainsChild(Inline childToFind)
	{
		Inline inline = FirstChild;
		while (inline != null)
		{
			Inline nextSibling = inline.NextSibling;
			if (inline == childToFind)
			{
				return true;
			}
			inline = nextSibling;
		}
		return false;
	}

	public IEnumerable<T> FindDescendants<T>() where T : Inline
	{
		if (FirstChild == null)
		{
			return Array.Empty<T>();
		}
		return FindDescendantsInternal<T>();
	}

	internal IEnumerable<T> FindDescendantsInternal<T>() where T : MarkdownObject
	{
		Stack<Inline> stack = new Stack<Inline>();
		for (Inline child = LastChild; child != null; child = child.PreviousSibling)
		{
			stack.Push(child);
		}
		while (stack.Count > 0)
		{
			Inline child = stack.Pop();
			if (child is T val)
			{
				yield return val;
			}
			if (child is ContainerInline containerInline)
			{
				for (child = containerInline.LastChild; child != null; child = child.PreviousSibling)
				{
					stack.Push(child);
				}
			}
		}
	}

	public void TransferChildrenTo(ContainerInline destination)
	{
		if (destination == null)
		{
			ThrowHelper.ArgumentNullException("destination");
		}
		if (this != destination)
		{
			Inline inline = FirstChild;
			while (inline != null)
			{
				Inline? nextSibling = inline.NextSibling;
				inline.Remove();
				destination.AppendChild(inline);
				inline = nextSibling;
			}
		}
	}

	public void MoveChildrenAfter(Inline parent)
	{
		if (parent == null)
		{
			ThrowHelper.ArgumentNullException("parent");
		}
		Inline inline = FirstChild;
		Inline inline2 = parent;
		while (inline != null)
		{
			Inline? nextSibling = inline.NextSibling;
			inline.Remove();
			inline2.InsertAfter(inline);
			inline2 = inline;
			inline = nextSibling;
		}
	}

	public void EmbraceChildrenBy(ContainerInline container)
	{
		if (container == null)
		{
			ThrowHelper.ArgumentNullException("container");
		}
		Inline inline = FirstChild;
		while (inline != null)
		{
			Inline? nextSibling = inline.NextSibling;
			inline.Remove();
			container.AppendChild(inline);
			inline = nextSibling;
		}
		AppendChild(container);
	}

	protected override void OnChildInsert(Inline child)
	{
		if (child.PreviousSibling == null && child.NextSibling == FirstChild)
		{
			FirstChild = child;
		}
		else if (child.NextSibling == null && child.PreviousSibling == LastChild)
		{
			LastChild = child;
		}
		if (LastChild == null)
		{
			LastChild = FirstChild;
		}
		else if (FirstChild == null)
		{
			FirstChild = LastChild;
		}
	}

	protected override void OnChildRemove(Inline child)
	{
		if (child == FirstChild)
		{
			if (FirstChild == LastChild)
			{
				FirstChild = null;
				LastChild = null;
			}
			else
			{
				FirstChild = child.NextSibling ?? LastChild;
			}
		}
		else if (child == LastChild)
		{
			LastChild = child.PreviousSibling ?? FirstChild;
		}
	}

	protected override void DumpChildTo(TextWriter writer, int level)
	{
		if (FirstChild != null)
		{
			level++;
			FirstChild.DumpTo(writer, level);
		}
	}

	public bool HasValidSpan(bool recursive = false)
	{
		for (Inline inline = FirstChild; inline != null; inline = inline.NextSibling)
		{
			if (!ContainsSpan(in Span, in inline.Span))
			{
				return false;
			}
			if (recursive && inline is ContainerInline containerInline && !containerInline.HasValidSpan(recursive: true))
			{
				return false;
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
		for (Inline inline = FirstChild; inline != null; inline = inline.NextSibling)
		{
			if (recursive && inline is ContainerInline containerInline)
			{
				containerInline.UpdateSpanFromChildren(recursive: true, preserveSelfSpan);
			}
			AppendSpan(ref destinationSpan, ref hasDestinationSpan, in inline.Span);
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

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<Inline> IEnumerable<Inline>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

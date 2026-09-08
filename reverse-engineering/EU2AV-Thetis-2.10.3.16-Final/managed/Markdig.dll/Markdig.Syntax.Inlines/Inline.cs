using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Markdig.Helpers;

namespace Markdig.Syntax.Inlines;

public abstract class Inline : MarkdownObject, IInline, IMarkdownObject
{
	public ContainerInline? Parent { get; internal set; }

	public Inline? PreviousSibling { get; private set; }

	public Inline? NextSibling { get; internal set; }

	public bool IsClosed
	{
		get
		{
			return base.IsClosedInternal;
		}
		set
		{
			base.IsClosedInternal = value;
		}
	}

	protected Inline()
	{
		SetTypeKind(isInline: true, isContainer: false);
	}

	private protected Inline(bool dummySkipTypeKind)
	{
	}

	public void InsertAfter(Inline next)
	{
		if (next == null)
		{
			ThrowHelper.ArgumentNullException("next");
		}
		if (next.Parent != null)
		{
			ThrowHelper.ArgumentException("Inline has already a parent", "next");
		}
		Inline nextSibling = NextSibling;
		if (nextSibling != null)
		{
			nextSibling.PreviousSibling = next;
		}
		next.PreviousSibling = this;
		next.NextSibling = nextSibling;
		NextSibling = next;
		if (Parent != null)
		{
			Parent.OnChildInsert(next);
			next.Parent = Parent;
		}
	}

	public void InsertBefore(Inline previous)
	{
		if (previous == null)
		{
			ThrowHelper.ArgumentNullException("previous");
		}
		if (previous.Parent != null)
		{
			ThrowHelper.ArgumentException("Inline has already a parent", "previous");
		}
		Inline previousSibling = PreviousSibling;
		if (previousSibling != null)
		{
			previousSibling.NextSibling = previous;
		}
		PreviousSibling = previous;
		previous.NextSibling = this;
		if (Parent != null)
		{
			Parent.OnChildInsert(previous);
			previous.Parent = Parent;
		}
	}

	public void Remove()
	{
		if (PreviousSibling != null)
		{
			PreviousSibling.NextSibling = NextSibling;
		}
		if (NextSibling != null)
		{
			NextSibling.PreviousSibling = PreviousSibling;
		}
		if (Parent != null)
		{
			Parent.OnChildRemove(this);
			PreviousSibling = null;
			NextSibling = null;
			Parent = null;
		}
	}

	public Inline ReplaceBy(Inline inline, bool copyChildren = true)
	{
		if (inline == null)
		{
			ThrowHelper.ArgumentNullException("inline");
		}
		ContainerInline parent = Parent;
		Inline previousSibling = PreviousSibling;
		Inline nextSibling = NextSibling;
		Remove();
		if (previousSibling != null)
		{
			previousSibling.InsertAfter(inline);
		}
		else if (nextSibling != null)
		{
			nextSibling.InsertBefore(inline);
		}
		else
		{
			parent?.AppendChild(inline);
		}
		if (copyChildren && base.IsContainerInline)
		{
			ContainerInline containerInline = Unsafe.As<ContainerInline>(this);
			ContainerInline containerInline2 = ((inline.IsContainerInline && !inline.IsClosed) ? Unsafe.As<ContainerInline>(inline) : null);
			Inline inline2 = containerInline.FirstChild;
			Inline inline3 = inline;
			while (inline2 != null)
			{
				Inline? nextSibling2 = inline2.NextSibling;
				inline2.Remove();
				if (containerInline2 != null)
				{
					containerInline2.AppendChild(inline2);
				}
				else
				{
					inline3.InsertAfter(inline2);
				}
				inline3 = inline2;
				inline2 = nextSibling2;
			}
			return inline3;
		}
		return inline;
	}

	public bool ContainsParentOfType<T>() where T : Inline
	{
		for (Inline inline = this; inline != null; inline = inline.Parent)
		{
			if (inline as T != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsParentOrSiblingOfType<T>() where T : Inline
	{
		if (ContainsParentOfType<T>())
		{
			return true;
		}
		ContainerInline parent = Parent;
		while (parent?.Parent != null)
		{
			parent = parent.Parent;
		}
		if (parent != null)
		{
			ContainerInline containerInline = parent;
			for (Inline inline = containerInline.FirstChild; inline != null; inline = inline.NextSibling)
			{
				if (inline is T)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public IEnumerable<T> FindParentOfType<T>() where T : Inline
	{
		for (Inline inline = this; inline != null; inline = inline.Parent)
		{
			if (inline is T val)
			{
				yield return val;
			}
		}
	}

	public T? FirstParentOfType<T>() where T : notnull, Inline
	{
		for (Inline inline = this; inline != null; inline = inline.Parent)
		{
			if (inline is T result)
			{
				return result;
			}
		}
		return null;
	}

	public Inline FindBestParent()
	{
		Inline inline = this;
		while (inline.Parent != null || inline.PreviousSibling != null)
		{
			inline = ((inline.Parent == null) ? inline.PreviousSibling : inline.Parent);
		}
		return inline;
	}

	protected virtual void OnChildRemove(Inline child)
	{
	}

	protected virtual void OnChildInsert(Inline child)
	{
	}

	public void DumpTo(TextWriter writer)
	{
		if (writer == null)
		{
			ThrowHelper.ArgumentNullException_writer();
		}
		DumpTo(writer, 0);
	}

	public void DumpTo(TextWriter writer, int level)
	{
		if (writer == null)
		{
			ThrowHelper.ArgumentNullException_writer();
		}
		for (int i = 0; i < level; i++)
		{
			writer.Write(' ');
		}
		writer.WriteLine("-> " + GetType().Name + " = " + this);
		DumpChildTo(writer, level + 1);
		if (NextSibling != null)
		{
			NextSibling.DumpTo(writer, level);
		}
	}

	protected virtual void DumpChildTo(TextWriter writer, int level)
	{
	}
}

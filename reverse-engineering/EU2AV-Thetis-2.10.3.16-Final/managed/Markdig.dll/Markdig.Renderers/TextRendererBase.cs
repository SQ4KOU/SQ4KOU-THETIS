using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers;

public abstract class TextRendererBase : RendererBase
{
	private TextWriter _writer;

	public TextWriter Writer
	{
		get
		{
			return _writer;
		}
		[MemberNotNull("_writer")]
		set
		{
			if (value == null)
			{
				ThrowHelper.ArgumentNullException("value");
			}
			value.NewLine = "\n";
			_writer = value;
		}
	}

	protected TextRendererBase(TextWriter writer)
	{
		Writer = writer;
	}

	public override object Render(MarkdownObject markdownObject)
	{
		Write(markdownObject);
		return Writer;
	}
}
public abstract class TextRendererBase<T> : TextRendererBase where T : TextRendererBase<T>
{
	private sealed class Indent
	{
		private readonly string? _constant;

		private readonly string[]? _lineSpecific;

		private int position;

		internal Indent(string constant)
		{
			_constant = constant;
		}

		internal Indent(string[] lineSpecific)
		{
			_lineSpecific = lineSpecific;
		}

		internal string Next()
		{
			if (_constant != null)
			{
				return _constant;
			}
			if (position == _lineSpecific.Length)
			{
				return string.Empty;
			}
			return _lineSpecific[position++];
		}
	}

	protected bool previousWasLine;

	private char[] buffer;

	private readonly List<Indent> indents;

	protected TextRendererBase(TextWriter writer)
		: base(writer)
	{
		buffer = new char[1024];
		previousWasLine = true;
		indents = new List<Indent>();
	}

	protected internal void Reset()
	{
		if (base.Writer is StringWriter stringWriter)
		{
			stringWriter.GetStringBuilder().Length = 0;
		}
		else
		{
			ThrowHelper.InvalidOperationException("Cannot reset this TextWriter instance");
		}
		ResetInternal();
	}

	internal void ResetInternal()
	{
		_childrenDepth = 0;
		previousWasLine = true;
		indents.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T EnsureLine()
	{
		if (!previousWasLine)
		{
			previousWasLine = true;
			base.Writer.WriteLine();
		}
		return (T)this;
	}

	public void PushIndent(string indent)
	{
		if (indent == null)
		{
			ThrowHelper.ArgumentNullException("indent");
		}
		indents.Add(new Indent(indent));
	}

	public void PushIndent(string[] lineSpecific)
	{
		if (indents == null)
		{
			ThrowHelper.ArgumentNullException("indents");
		}
		indents.Add(new Indent(lineSpecific));
		previousWasLine = true;
	}

	public void PopIndent()
	{
		if (indents.Count > 0)
		{
			indents.RemoveAt(indents.Count - 1);
			return;
		}
		throw new InvalidOperationException("No indent to pop");
	}

	public void ClearIndent()
	{
		indents.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected void WriteIndent()
	{
		if (previousWasLine)
		{
			WriteIndentCore();
		}
	}

	private void WriteIndentCore()
	{
		previousWasLine = false;
		for (int i = 0; i < indents.Count; i++)
		{
			string value = indents[i].Next();
			base.Writer.Write(value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Write(string? content)
	{
		WriteIndent();
		base.Writer.Write(content);
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal T Write(char c, int count)
	{
		WriteIndent();
		for (int i = 0; i < count; i++)
		{
			base.Writer.Write(c);
		}
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Write(ref StringSlice slice)
	{
		Write(slice.AsSpan());
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Write(StringSlice slice)
	{
		Write(slice.AsSpan());
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Write(char content)
	{
		WriteIndent();
		if (content == '\n')
		{
			previousWasLine = true;
		}
		base.Writer.Write(content);
		return (T)this;
	}

	public T Write(string content, int offset, int length)
	{
		if (content != null)
		{
			Write(content.AsSpan(offset, length));
		}
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write(ReadOnlySpan<char> content)
	{
		if (!content.IsEmpty)
		{
			WriteIndent();
			WriteRaw(content);
			if (content[content.Length - 1] == '\n')
			{
				previousWasLine = true;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteRaw(char content)
	{
		base.Writer.Write(content);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteRaw(string? content)
	{
		base.Writer.Write(content);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteRaw(ReadOnlySpan<char> content)
	{
		if (content.Length > buffer.Length)
		{
			buffer = content.ToArray();
		}
		else
		{
			content.CopyTo(buffer);
		}
		base.Writer.Write(buffer, 0, content.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T WriteLine()
	{
		WriteIndent();
		base.Writer.WriteLine();
		previousWasLine = true;
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T WriteLine(NewLine newLine)
	{
		WriteIndent();
		base.Writer.Write(newLine.AsString());
		previousWasLine = true;
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T WriteLine(string content)
	{
		WriteIndent();
		previousWasLine = true;
		base.Writer.WriteLine(content);
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T WriteLine(char content)
	{
		WriteIndent();
		previousWasLine = true;
		base.Writer.WriteLine(content);
		return (T)this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T WriteLeafInline(LeafBlock leafBlock)
	{
		if (leafBlock == null)
		{
			ThrowHelper.ArgumentNullException_leafBlock();
		}
		for (Inline inline = leafBlock.Inline; inline != null; inline = inline.NextSibling)
		{
			Write(inline);
		}
		return (T)this;
	}
}

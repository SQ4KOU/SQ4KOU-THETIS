using System;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Renderers;

public abstract class MarkdownObjectRenderer<TRenderer, TObject> : IMarkdownObjectRenderer where TRenderer : RendererBase where TObject : MarkdownObject
{
	public delegate bool TryWriteDelegate(TRenderer renderer, TObject obj);

	private OrderedList<TryWriteDelegate>? _tryWriters;

	public OrderedList<TryWriteDelegate> TryWriters => _tryWriters ?? (_tryWriters = new OrderedList<TryWriteDelegate>());

	public bool Accept(RendererBase renderer, Type objectType)
	{
		return typeof(TObject).IsAssignableFrom(objectType);
	}

	public virtual void Write(RendererBase renderer, MarkdownObject obj)
	{
		TRenderer renderer2 = (TRenderer)renderer;
		TObject obj2 = (TObject)obj;
		if (_tryWriters == null || !TryWrite(renderer2, obj2))
		{
			Write(renderer2, obj2);
		}
	}

	private bool TryWrite(TRenderer renderer, TObject obj)
	{
		for (int i = 0; i < _tryWriters.Count; i++)
		{
			if (_tryWriters[i](renderer, obj))
			{
				return true;
			}
		}
		return false;
	}

	protected abstract void Write(TRenderer renderer, TObject obj);
}

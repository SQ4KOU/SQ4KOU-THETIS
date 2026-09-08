using System;
using System.Runtime.CompilerServices;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers;

public abstract class RendererBase : IMarkdownRenderer
{
	private readonly struct RendererEntry(IntPtr key, IMarkdownObjectRenderer? renderer)
	{
		public readonly IntPtr Key = key;

		public readonly IMarkdownObjectRenderer? Renderer = renderer;
	}

	private const int SubTableCount = 32;

	private readonly RendererEntry[][] _renderersPerType;

	internal int _childrenDepth;

	public ObjectRendererCollection ObjectRenderers { get; } = new ObjectRendererCollection();

	public bool IsFirstInContainer { get; private set; }

	public bool IsLastInContainer { get; private set; }

	public event Action<IMarkdownRenderer, MarkdownObject>? ObjectWriteBefore;

	public event Action<IMarkdownRenderer, MarkdownObject>? ObjectWriteAfter;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static IntPtr GetKeyForType(MarkdownObject obj)
	{
		return Type.GetTypeHandle(obj).Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int SubTableIndex(IntPtr key)
	{
		return (int)(((ulong)(long)key / 64uL) & 0x1F);
	}

	protected RendererBase()
	{
		RendererEntry[][] array = (_renderersPerType = new RendererEntry[32][]);
		for (int i = 0; i < array.Length; i++)
		{
			RendererEntry[][] array2 = array;
			int num = i;
			if (array2[num] == null)
			{
				array2[num] = Array.Empty<RendererEntry>();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private IMarkdownObjectRenderer? GetRendererInstance(MarkdownObject obj)
	{
		Type type = obj.GetType();
		IMarkdownObjectRenderer markdownObjectRenderer = null;
		foreach (IMarkdownObjectRenderer objectRenderer in ObjectRenderers)
		{
			if (objectRenderer.Accept(this, type))
			{
				markdownObjectRenderer = objectRenderer;
				break;
			}
		}
		IntPtr keyForType = GetKeyForType(obj);
		ref RendererEntry[] reference = ref _renderersPerType[SubTableIndex(keyForType)];
		Array.Resize(ref reference, reference.Length + 1);
		reference[reference.Length - 1] = new RendererEntry(keyForType, markdownObjectRenderer);
		return markdownObjectRenderer;
	}

	public abstract object Render(MarkdownObject markdownObject);

	public void WriteChildren(ContainerBlock containerBlock)
	{
		if (containerBlock != null)
		{
			ThrowHelper.CheckDepthLimit(_childrenDepth++);
			bool isFirstInContainer = IsFirstInContainer;
			bool isLastInContainer = IsLastInContainer;
			for (int i = 0; i < containerBlock.Count; i++)
			{
				IsFirstInContainer = i == 0;
				IsLastInContainer = i + 1 == containerBlock.Count;
				Write(containerBlock[i]);
			}
			IsFirstInContainer = isFirstInContainer;
			IsLastInContainer = isLastInContainer;
			_childrenDepth--;
		}
	}

	public void WriteChildren(ContainerInline containerInline)
	{
		if (containerInline != null)
		{
			ThrowHelper.CheckDepthLimit(_childrenDepth++);
			bool isFirstInContainer = IsFirstInContainer;
			bool isLastInContainer = IsLastInContainer;
			bool isFirstInContainer2 = true;
			Inline inline = containerInline.FirstChild;
			while (inline != null)
			{
				IsFirstInContainer = isFirstInContainer2;
				IsLastInContainer = inline.NextSibling == null;
				Write(inline);
				inline = inline.NextSibling;
				isFirstInContainer2 = false;
			}
			IsFirstInContainer = isFirstInContainer;
			IsLastInContainer = isLastInContainer;
			_childrenDepth--;
		}
	}

	public void Write(MarkdownObject obj)
	{
		if (obj == null)
		{
			return;
		}
		ObjectWriteBefore?.Invoke(this, obj);
		IMarkdownObjectRenderer markdownObjectRenderer = null;
		IntPtr keyForType = GetKeyForType(obj);
		RendererEntry[] array = _renderersPerType[SubTableIndex(keyForType)];
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				RendererEntry rendererEntry = array[num];
				if (keyForType == rendererEntry.Key)
				{
					markdownObjectRenderer = rendererEntry.Renderer;
					break;
				}
				num++;
				continue;
			}
			markdownObjectRenderer = GetRendererInstance(obj);
			break;
		}
		if (markdownObjectRenderer != null)
		{
			markdownObjectRenderer.Write(this, obj);
		}
		else if (obj.IsContainerInline)
		{
			WriteChildren(Unsafe.As<ContainerInline>(obj));
		}
		else if (obj.IsContainerBlock)
		{
			WriteChildren(Unsafe.As<ContainerBlock>(obj));
		}
		ObjectWriteAfter?.Invoke(this, obj);
	}
}

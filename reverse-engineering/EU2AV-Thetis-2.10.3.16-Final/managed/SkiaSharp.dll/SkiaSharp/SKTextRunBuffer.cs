using System;

namespace SkiaSharp;

public class SKTextRunBuffer : SKRunBuffer
{
	public int TextSize { get; }

	public unsafe Span<byte> Text => new Span<byte>(internalBuffer.utf8text, TextSize);

	public unsafe Span<uint> Clusters => new Span<uint>(internalBuffer.clusters, base.Size);

	internal SKTextRunBuffer(SKRunBufferInternal buffer, int size, int textSize)
		: base(buffer, size)
	{
		TextSize = textSize;
	}

	public void SetText(ReadOnlySpan<byte> text)
	{
		text.CopyTo(Text);
	}

	public void SetClusters(ReadOnlySpan<uint> clusters)
	{
		clusters.CopyTo(Clusters);
	}
}

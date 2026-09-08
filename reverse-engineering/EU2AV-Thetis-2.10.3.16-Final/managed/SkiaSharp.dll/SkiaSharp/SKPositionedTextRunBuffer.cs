using System;

namespace SkiaSharp;

public sealed class SKPositionedTextRunBuffer : SKTextRunBuffer
{
	public unsafe Span<SKPoint> Positions => new Span<SKPoint>(internalBuffer.pos, base.Size);

	internal SKPositionedTextRunBuffer(SKRunBufferInternal buffer, int size, int textSize)
		: base(buffer, size, textSize)
	{
	}

	public void SetPositions(ReadOnlySpan<SKPoint> positions)
	{
		positions.CopyTo(Positions);
	}
}

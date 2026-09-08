using System;

namespace SkiaSharp;

public sealed class SKHorizontalTextRunBuffer : SKTextRunBuffer
{
	public unsafe Span<float> Positions => new Span<float>(internalBuffer.pos, base.Size);

	internal SKHorizontalTextRunBuffer(SKRunBufferInternal buffer, int size, int textSize)
		: base(buffer, size, textSize)
	{
	}

	public void SetPositions(ReadOnlySpan<float> positions)
	{
		positions.CopyTo(Positions);
	}
}

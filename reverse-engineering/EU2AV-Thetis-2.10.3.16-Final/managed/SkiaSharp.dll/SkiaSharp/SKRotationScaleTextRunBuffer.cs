using System;

namespace SkiaSharp;

public sealed class SKRotationScaleTextRunBuffer : SKTextRunBuffer
{
	public unsafe Span<SKRotationScaleMatrix> Positions => new Span<SKRotationScaleMatrix>(internalBuffer.pos, base.Size);

	internal SKRotationScaleTextRunBuffer(SKRunBufferInternal buffer, int size, int textSize)
		: base(buffer, size, textSize)
	{
	}

	public void SetPositions(ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		positions.CopyTo(Positions);
	}
}

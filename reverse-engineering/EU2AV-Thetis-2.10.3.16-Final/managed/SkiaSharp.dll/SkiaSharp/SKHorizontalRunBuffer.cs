using System;

namespace SkiaSharp;

public sealed class SKHorizontalRunBuffer : SKRunBuffer
{
	public unsafe Span<float> Positions => new Span<float>(internalBuffer.pos, base.Size);

	internal SKHorizontalRunBuffer(SKRunBufferInternal buffer, int size)
		: base(buffer, size)
	{
	}

	public void SetPositions(ReadOnlySpan<float> positions)
	{
		positions.CopyTo(Positions);
	}

	[Obsolete("Use Positions instead.")]
	public Span<float> GetPositionSpan()
	{
		return Positions;
	}
}

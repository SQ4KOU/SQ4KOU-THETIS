using System;

namespace SkiaSharp;

public sealed class SKPositionedRunBuffer : SKRunBuffer
{
	public unsafe Span<SKPoint> Positions => new Span<SKPoint>(internalBuffer.pos, base.Size);

	internal SKPositionedRunBuffer(SKRunBufferInternal buffer, int size)
		: base(buffer, size)
	{
	}

	public void SetPositions(ReadOnlySpan<SKPoint> positions)
	{
		positions.CopyTo(Positions);
	}

	[Obsolete("Use Positions instead.")]
	public Span<SKPoint> GetPositionSpan()
	{
		return Positions;
	}
}

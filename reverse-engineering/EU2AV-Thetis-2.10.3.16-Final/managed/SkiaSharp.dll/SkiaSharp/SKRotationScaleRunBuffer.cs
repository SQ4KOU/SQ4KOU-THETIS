using System;

namespace SkiaSharp;

public sealed class SKRotationScaleRunBuffer : SKRunBuffer
{
	public unsafe Span<SKRotationScaleMatrix> Positions => new Span<SKRotationScaleMatrix>(internalBuffer.pos, base.Size);

	internal SKRotationScaleRunBuffer(SKRunBufferInternal buffer, int size)
		: base(buffer, size)
	{
	}

	public void SetPositions(ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		positions.CopyTo(Positions);
	}

	[Obsolete("Use Positions instead.")]
	public Span<SKRotationScaleMatrix> GetRotationScaleSpan()
	{
		return Positions;
	}

	[Obsolete("Use SetPositions instead.")]
	public void SetRotationScale(ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		SetPositions(positions);
	}
}

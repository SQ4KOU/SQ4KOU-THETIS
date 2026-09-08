using System;

namespace SkiaSharp;

public readonly struct SKRawRunBuffer<T>
{
	internal readonly SKRunBufferInternal buffer;

	private readonly int size;

	private readonly int posSize;

	private readonly int textSize;

	public unsafe Span<ushort> Glyphs => new Span<ushort>(buffer.glyphs, size);

	public unsafe Span<T> Positions => new Span<T>(buffer.pos, posSize);

	public unsafe Span<byte> Text => new Span<byte>(buffer.utf8text, textSize);

	public unsafe Span<uint> Clusters => new Span<uint>(buffer.clusters, size);

	internal SKRawRunBuffer(SKRunBufferInternal buffer, int size, int posSize, int textSize)
	{
		this.buffer = buffer;
		this.size = size;
		this.posSize = posSize;
		this.textSize = textSize;
	}
}

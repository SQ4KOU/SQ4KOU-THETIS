using System;

namespace SkiaSharp;

public class SKRunBuffer
{
	internal readonly SKRunBufferInternal internalBuffer;

	public int Size { get; }

	public unsafe Span<ushort> Glyphs => new Span<ushort>(internalBuffer.glyphs, Size);

	internal SKRunBuffer(SKRunBufferInternal buffer, int size)
	{
		internalBuffer = buffer;
		Size = size;
	}

	public void SetGlyphs(ReadOnlySpan<ushort> glyphs)
	{
		glyphs.CopyTo(Glyphs);
	}

	[Obsolete("Use Glyphs instead.")]
	public Span<ushort> GetGlyphSpan()
	{
		return Glyphs;
	}
}

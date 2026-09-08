using System;

namespace SkiaSharp;

public class SKTextBlobBuilder : SKObject, ISKSkipObjectRegistration
{
	internal SKTextBlobBuilder(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	public SKTextBlobBuilder()
		: this(SkiaApi.sk_textblob_builder_new(), owns: true)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_textblob_builder_delete(Handle);
	}

	public SKTextBlob? Build()
	{
		SKTextBlob result = SKTextBlob.GetObject(SkiaApi.sk_textblob_builder_make(Handle));
		GC.KeepAlive(this);
		return result;
	}

	public void AddRun(ReadOnlySpan<ushort> glyphs, SKFont font, SKPoint origin = default(SKPoint))
	{
		SKRawRunBuffer<SKPoint> sKRawRunBuffer = AllocateRawPositionedRun(font, glyphs.Length);
		glyphs.CopyTo(sKRawRunBuffer.Glyphs);
		font.GetGlyphPositions(sKRawRunBuffer.Glyphs, sKRawRunBuffer.Positions, origin);
	}

	public void AddHorizontalRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> positions, float y)
	{
		SKRawRunBuffer<float> sKRawRunBuffer = AllocateRawHorizontalRun(font, glyphs.Length, y);
		glyphs.CopyTo(sKRawRunBuffer.Glyphs);
		positions.CopyTo(sKRawRunBuffer.Positions);
	}

	public void AddPositionedRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKPoint> positions)
	{
		SKRawRunBuffer<SKPoint> sKRawRunBuffer = AllocateRawPositionedRun(font, glyphs.Length);
		glyphs.CopyTo(sKRawRunBuffer.Glyphs);
		positions.CopyTo(sKRawRunBuffer.Positions);
	}

	public void AddRotationScaleRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		SKRawRunBuffer<SKRotationScaleMatrix> sKRawRunBuffer = AllocateRawRotationScaleRun(font, glyphs.Length);
		glyphs.CopyTo(sKRawRunBuffer.Glyphs);
		positions.CopyTo(sKRawRunBuffer.Positions);
	}

	public void AddPathPositionedRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> glyphWidths, ReadOnlySpan<SKPoint> glyphOffsets, SKPath path, SKTextAlign textAlign = SKTextAlign.Left)
	{
		using SKPathMeasure sKPathMeasure = new SKPathMeasure(path);
		float length = sKPathMeasure.Length;
		float num = glyphOffsets[glyphs.Length - 1].X + glyphWidths[glyphs.Length - 1];
		float num2 = (float)textAlign * 0.5f;
		float num3 = glyphOffsets[0].X + (length - num) * num2;
		int start = 0;
		int num4 = 0;
		Utils.RentedArray<SKRotationScaleMatrix> rentedArray = Utils.RentArray<SKRotationScaleMatrix>(glyphs.Length);
		try
		{
			for (int i = 0; i < glyphOffsets.Length; i++)
			{
				SKPoint sKPoint = glyphOffsets[i];
				float num5 = glyphWidths[i] * 0.5f;
				float num6 = num3 + sKPoint.X + num5;
				if (num6 >= 0f && num6 < length && sKPathMeasure.GetPositionAndTangent(num6, out var position, out var tangent))
				{
					if (num4 == 0)
					{
						start = i;
					}
					float x = tangent.X;
					float y = tangent.Y;
					float x2 = position.X;
					float y2 = position.Y;
					x2 -= x * num5;
					y2 -= y * num5;
					float y3 = sKPoint.Y;
					x2 -= y3 * y;
					y2 += y3 * x;
					rentedArray.Span[num4++] = new SKRotationScaleMatrix(x, y, x2, y2);
				}
			}
			ReadOnlySpan<ushort> glyphs2 = glyphs.Slice(start, num4);
			Span<SKRotationScaleMatrix> span = rentedArray.Span.Slice(0, num4);
			AddRotationScaleRun(glyphs2, font, span);
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	public SKRunBuffer AllocateRun(SKFont font, int count, float x, float y, SKRect? bounds = null)
	{
		return new SKRunBuffer(AllocateRawRun(font, count, x, y, bounds).buffer, count);
	}

	public unsafe SKRawRunBuffer<float> AllocateRawRun(SKFont font, int count, float x, float y, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run(Handle, font.Handle, count, x, y, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run(Handle, font.Handle, count, x, y, null, &buffer);
		}
		return new SKRawRunBuffer<float>(buffer, count, 0, 0);
	}

	public SKTextRunBuffer AllocateTextRun(SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds = null)
	{
		return new SKTextRunBuffer(AllocateRawTextRun(font, count, x, y, textByteCount, bounds).buffer, count, textByteCount);
	}

	public unsafe SKRawRunBuffer<float> AllocateRawTextRun(SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_text(Handle, font.Handle, count, x, y, textByteCount, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_text(Handle, font.Handle, count, x, y, textByteCount, null, &buffer);
		}
		return new SKRawRunBuffer<float>(buffer, count, 0, textByteCount);
	}

	public SKHorizontalRunBuffer AllocateHorizontalRun(SKFont font, int count, float y, SKRect? bounds = null)
	{
		return new SKHorizontalRunBuffer(AllocateRawHorizontalRun(font, count, y, bounds).buffer, count);
	}

	public unsafe SKRawRunBuffer<float> AllocateRawHorizontalRun(SKFont font, int count, float y, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_pos_h(Handle, font.Handle, count, y, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_pos_h(Handle, font.Handle, count, y, null, &buffer);
		}
		return new SKRawRunBuffer<float>(buffer, count, count, 0);
	}

	public SKHorizontalTextRunBuffer AllocateHorizontalTextRun(SKFont font, int count, float y, int textByteCount, SKRect? bounds = null)
	{
		return new SKHorizontalTextRunBuffer(AllocateRawHorizontalTextRun(font, count, y, textByteCount, bounds).buffer, count, textByteCount);
	}

	public unsafe SKRawRunBuffer<float> AllocateRawHorizontalTextRun(SKFont font, int count, float y, int textByteCount, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_text_pos_h(Handle, font.Handle, count, y, textByteCount, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_text_pos_h(Handle, font.Handle, count, y, textByteCount, null, &buffer);
		}
		return new SKRawRunBuffer<float>(buffer, count, count, textByteCount);
	}

	public SKPositionedRunBuffer AllocatePositionedRun(SKFont font, int count, SKRect? bounds = null)
	{
		return new SKPositionedRunBuffer(AllocateRawPositionedRun(font, count, bounds).buffer, count);
	}

	public unsafe SKRawRunBuffer<SKPoint> AllocateRawPositionedRun(SKFont font, int count, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_pos(Handle, font.Handle, count, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_pos(Handle, font.Handle, count, null, &buffer);
		}
		return new SKRawRunBuffer<SKPoint>(buffer, count, count, 0);
	}

	public SKPositionedTextRunBuffer AllocatePositionedTextRun(SKFont font, int count, int textByteCount, SKRect? bounds = null)
	{
		return new SKPositionedTextRunBuffer(AllocateRawPositionedTextRun(font, count, textByteCount, bounds).buffer, count, textByteCount);
	}

	public unsafe SKRawRunBuffer<SKPoint> AllocateRawPositionedTextRun(SKFont font, int count, int textByteCount, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_text_pos(Handle, font.Handle, count, textByteCount, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_text_pos(Handle, font.Handle, count, textByteCount, null, &buffer);
		}
		return new SKRawRunBuffer<SKPoint>(buffer, count, count, textByteCount);
	}

	public SKRotationScaleRunBuffer AllocateRotationScaleRun(SKFont font, int count, SKRect? bounds = null)
	{
		return new SKRotationScaleRunBuffer(AllocateRawRotationScaleRun(font, count, bounds).buffer, count);
	}

	public unsafe SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleRun(SKFont font, int count, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_rsxform(Handle, font.Handle, count, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_rsxform(Handle, font.Handle, count, null, &buffer);
		}
		return new SKRawRunBuffer<SKRotationScaleMatrix>(buffer, count, count, 0);
	}

	public SKRotationScaleTextRunBuffer AllocateRotationScaleTextRun(SKFont font, int count, int textByteCount, SKRect? bounds = null)
	{
		return new SKRotationScaleTextRunBuffer(AllocateRawRotationScaleTextRun(font, count, textByteCount, bounds).buffer, count, textByteCount);
	}

	public unsafe SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleTextRun(SKFont font, int count, int textByteCount, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_text_rsxform(Handle, font.Handle, count, textByteCount, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_text_rsxform(Handle, font.Handle, count, textByteCount, null, &buffer);
		}
		return new SKRawRunBuffer<SKRotationScaleMatrix>(buffer, count, count, textByteCount);
	}
}

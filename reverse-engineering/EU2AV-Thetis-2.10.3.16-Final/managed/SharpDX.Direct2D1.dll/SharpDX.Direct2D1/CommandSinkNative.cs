using System;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("54d7898a-a061-40a7-bec7-e465bcba2c4f")]
internal class CommandSinkNative : ComObject, CommandSink, IUnknown, ICallbackable, IDisposable
{
	public AntialiasMode AntialiasMode
	{
		set
		{
			SetAntialiasMode_(value);
		}
	}

	public TextAntialiasMode TextAntialiasMode
	{
		set
		{
			SetTextAntialiasMode_(value);
		}
	}

	public RenderingParams TextRenderingParams
	{
		set
		{
			SetTextRenderingParams_(value);
		}
	}

	public RawMatrix3x2 Transform
	{
		set
		{
			SetTransform_(ref value);
		}
	}

	public PrimitiveBlend PrimitiveBlend
	{
		set
		{
			SetPrimitiveBlend_(value);
		}
	}

	public UnitMode UnitMode
	{
		set
		{
			SetUnitMode_(value);
		}
	}

	public AntialiasMode AntialiasMode_
	{
		set
		{
			SetAntialiasMode_(value);
		}
	}

	public TextAntialiasMode TextAntialiasMode_
	{
		set
		{
			SetTextAntialiasMode_(value);
		}
	}

	public RenderingParams TextRenderingParams_
	{
		set
		{
			SetTextRenderingParams_(value);
		}
	}

	public RawMatrix3x2 Transform_
	{
		set
		{
			SetTransform_(ref value);
		}
	}

	public PrimitiveBlend PrimitiveBlend_
	{
		set
		{
			SetPrimitiveBlend_(value);
		}
	}

	public UnitMode UnitMode_
	{
		set
		{
			SetUnitMode_(value);
		}
	}

	public void BeginDraw()
	{
		BeginDraw_();
	}

	public void EndDraw()
	{
		EndDraw_();
	}

	public void SetTags(long tag1, long tag2)
	{
		SetTags_(tag1, tag2);
	}

	public void Clear(RawColor4? color = null)
	{
		Clear_(color);
	}

	public void DrawGlyphRun(RawVector2 baselineOrigin, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, Brush foregroundBrush, MeasuringMode measuringMode)
	{
		DrawGlyphRun_(baselineOrigin, glyphRun, glyphRunDescription, foregroundBrush, measuringMode);
	}

	public void DrawLine(RawVector2 point0, RawVector2 point1, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		DrawLine_(point0, point1, brush, strokeWidth, strokeStyle);
	}

	public void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		DrawGeometry_(geometry, brush, strokeWidth, strokeStyle);
	}

	public void DrawRectangle(RawRectangleF rect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		DrawRectangle_(rect, brush, strokeWidth, strokeStyle);
	}

	public void DrawBitmap(Bitmap bitmap, RawRectangleF? destinationRectangle, float opacity, InterpolationMode interpolationMode, RawRectangleF? sourceRectangle, RawMatrix? erspectiveTransformRef)
	{
		DrawBitmap_(bitmap, destinationRectangle, opacity, interpolationMode, sourceRectangle, erspectiveTransformRef);
	}

	public void DrawImage(Image image, RawVector2? targetOffset, RawRectangleF? imageRectangle, InterpolationMode interpolationMode, CompositeMode compositeMode)
	{
		DrawImage_(image, targetOffset, imageRectangle, interpolationMode, compositeMode);
	}

	public void DrawGdiMetafile(GdiMetafile gdiMetafile, RawVector2? targetOffset)
	{
		DrawGdiMetafile_(gdiMetafile, targetOffset);
	}

	public void FillMesh(Mesh mesh, Brush brush)
	{
		FillMesh_(mesh, brush);
	}

	public void FillOpacityMask(Bitmap opacityMask, Brush brush, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle)
	{
		FillOpacityMask_(opacityMask, brush, destinationRectangle, sourceRectangle);
	}

	public void FillGeometry(Geometry geometry, Brush brush, Brush opacityBrush)
	{
		FillGeometry_(geometry, brush, opacityBrush);
	}

	public void FillRectangle(RawRectangleF rect, Brush brush)
	{
		FillRectangle_(rect, brush);
	}

	public void PushAxisAlignedClip(RawRectangleF clipRect, AntialiasMode antialiasMode)
	{
		PushAxisAlignedClip_(clipRect, antialiasMode);
	}

	public void PushLayer(ref LayerParameters1 layerParameters1, Layer layer)
	{
		PushLayer_(ref layerParameters1, layer);
	}

	public void PopAxisAlignedClip()
	{
		PopAxisAlignedClip_();
	}

	public void PopLayer()
	{
		PopLayer_();
	}

	public CommandSinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CommandSinkNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CommandSinkNative(nativePtr);
		}
		return null;
	}

	internal unsafe void BeginDraw_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void EndDraw_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void SetAntialiasMode_(AntialiasMode antialiasMode)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (int)antialiasMode)).CheckError();
	}

	internal unsafe void SetTags_(long tag1, long tag2)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, tag1, tag2)).CheckError();
	}

	internal unsafe void SetTextAntialiasMode_(TextAntialiasMode textAntialiasMode)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (int)textAntialiasMode)).CheckError();
	}

	internal unsafe void SetTextRenderingParams_(RenderingParams textRenderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<RenderingParams>(textRenderingParams);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void SetTransform_(ref RawMatrix3x2 transform)
	{
		Result result;
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void SetPrimitiveBlend_(PrimitiveBlend primitiveBlend)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (int)primitiveBlend)).CheckError();
	}

	internal unsafe void SetUnitMode_(UnitMode unitMode)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (int)unitMode)).CheckError();
	}

	internal unsafe void Clear_(RawColor4? color)
	{
		RawColor4 value = default(RawColor4);
		if (color.HasValue)
		{
			value = color.Value;
		}
		void* nativePointer = _nativePointer;
		RawColor4* intPtr = ((!color.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}

	internal unsafe void DrawGlyphRun_(RawVector2 baselineOrigin, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, Brush foregroundBrush, MeasuringMode measuringMode)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		GlyphRunDescription.__Native ref2 = default(GlyphRunDescription.__Native);
		IntPtr zero = IntPtr.Zero;
		glyphRun.__MarshalTo(ref @ref);
		glyphRunDescription?.__MarshalTo(ref ref2);
		zero = CppObject.ToCallbackPtr<Brush>(foregroundBrush);
		void* nativePointer = _nativePointer;
		Result result = ((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, baselineOrigin, &@ref, (glyphRunDescription == null) ? null : (&ref2), (void*)zero, (int)measuringMode);
		glyphRun.__MarshalFree(ref @ref);
		glyphRunDescription?.__MarshalFree(ref ref2);
		result.CheckError();
	}

	internal unsafe void DrawLine_(RawVector2 point0, RawVector2 point1, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		zero2 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		((Result)((delegate* unmanaged[Stdcall]<void*, RawVector2, RawVector2, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, point0, point1, (void*)zero, strokeWidth, (void*)zero2)).CheckError();
	}

	internal unsafe void DrawGeometry_(Geometry geometry, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(geometry);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		zero3 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, strokeWidth, (void*)zero3)).CheckError();
	}

	internal unsafe void DrawRectangle_(RawRectangleF rect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		zero2 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, &rect, (void*)zero, strokeWidth, (void*)zero2)).CheckError();
	}

	internal unsafe void DrawBitmap_(Bitmap bitmap, RawRectangleF? destinationRectangle, float opacity, InterpolationMode interpolationMode, RawRectangleF? sourceRectangle, RawMatrix? erspectiveTransformRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Bitmap>(bitmap);
		RawRectangleF value = default(RawRectangleF);
		if (destinationRectangle.HasValue)
		{
			value = destinationRectangle.Value;
		}
		RawRectangleF value2 = default(RawRectangleF);
		if (sourceRectangle.HasValue)
		{
			value2 = sourceRectangle.Value;
		}
		RawMatrix value3 = default(RawMatrix);
		if (erspectiveTransformRef.HasValue)
		{
			value3 = erspectiveTransformRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawRectangleF* intPtr2 = ((!destinationRectangle.HasValue) ? null : (&value));
		RawRectangleF* intPtr3 = ((!sourceRectangle.HasValue) ? null : (&value2));
		RawMatrix* intPtr4 = ((!erspectiveTransformRef.HasValue) ? null : (&value3));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, opacity, (int)interpolationMode, intPtr3, intPtr4)).CheckError();
	}

	internal unsafe void DrawImage_(Image image, RawVector2? targetOffset, RawRectangleF? imageRectangle, InterpolationMode interpolationMode, CompositeMode compositeMode)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(image);
		RawVector2 value = default(RawVector2);
		if (targetOffset.HasValue)
		{
			value = targetOffset.Value;
		}
		RawRectangleF value2 = default(RawRectangleF);
		if (imageRectangle.HasValue)
		{
			value2 = imageRectangle.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawVector2* intPtr2 = ((!targetOffset.HasValue) ? null : (&value));
		RawRectangleF* intPtr3 = ((!imageRectangle.HasValue) ? null : (&value2));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, (int)interpolationMode, (int)compositeMode)).CheckError();
	}

	internal unsafe void DrawGdiMetafile_(GdiMetafile gdiMetafile, RawVector2? targetOffset)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GdiMetafile>(gdiMetafile);
		RawVector2 value = default(RawVector2);
		if (targetOffset.HasValue)
		{
			value = targetOffset.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawVector2* intPtr2 = ((!targetOffset.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2)).CheckError();
	}

	internal unsafe void FillMesh_(Mesh mesh, Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Mesh>(mesh);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2)).CheckError();
	}

	internal unsafe void FillOpacityMask_(Bitmap opacityMask, Brush brush, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Bitmap>(opacityMask);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		RawRectangleF value = default(RawRectangleF);
		if (destinationRectangle.HasValue)
		{
			value = destinationRectangle.Value;
		}
		RawRectangleF value2 = default(RawRectangleF);
		if (sourceRectangle.HasValue)
		{
			value2 = sourceRectangle.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		void* intPtr2 = (void*)zero2;
		RawRectangleF* intPtr3 = ((!destinationRectangle.HasValue) ? null : (&value));
		RawRectangleF* intPtr4 = ((!sourceRectangle.HasValue) ? null : (&value2));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, intPtr4)).CheckError();
	}

	internal unsafe void FillGeometry_(Geometry geometry, Brush brush, Brush opacityBrush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(geometry);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		zero3 = CppObject.ToCallbackPtr<Brush>(opacityBrush);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, (void*)zero3)).CheckError();
	}

	internal unsafe void FillRectangle_(RawRectangleF rect, Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, &rect, (void*)zero)).CheckError();
	}

	internal unsafe void PushAxisAlignedClip_(RawRectangleF clipRect, AntialiasMode antialiasMode)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, &clipRect, (int)antialiasMode)).CheckError();
	}

	internal unsafe void PushLayer_(ref LayerParameters1 layerParameters1, Layer layer)
	{
		LayerParameters1.__Native @ref = default(LayerParameters1.__Native);
		IntPtr zero = IntPtr.Zero;
		layerParameters1.__MarshalTo(ref @ref);
		zero = CppObject.ToCallbackPtr<Layer>(layer);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, &@ref, (void*)zero);
		layerParameters1.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void PopAxisAlignedClip_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void PopLayer_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}

using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("e8f7fe7a-191c-466d-ad95-975678bda998")]
public class DeviceContext : RenderTarget
{
	public Device Device
	{
		get
		{
			GetDevice(out var device);
			return device;
		}
	}

	public Image Target
	{
		get
		{
			GetTarget(out var image);
			return image;
		}
		set
		{
			SetTarget(value);
		}
	}

	public RenderingControls RenderingControls
	{
		get
		{
			GetRenderingControls(out var renderingControls);
			return renderingControls;
		}
		set
		{
			SetRenderingControls(value);
		}
	}

	public PrimitiveBlend PrimitiveBlend
	{
		get
		{
			return GetPrimitiveBlend();
		}
		set
		{
			SetPrimitiveBlend(value);
		}
	}

	public UnitMode UnitMode
	{
		get
		{
			return GetUnitMode();
		}
		set
		{
			SetUnitMode(value);
		}
	}

	public DeviceContext(Surface surface)
		: base(IntPtr.Zero)
	{
		D2D1.CreateDeviceContext(surface, null, this);
	}

	public DeviceContext(Surface surface, CreationProperties creationProperties)
		: base(IntPtr.Zero)
	{
		D2D1.CreateDeviceContext(surface, creationProperties, this);
	}

	public DeviceContext(Device device, DeviceContextOptions options)
		: base(IntPtr.Zero)
	{
		device.CreateDeviceContext(options, this);
	}

	public void DrawImage(Effect effect, RawVector2 targetOffset, InterpolationMode interpolationMode = InterpolationMode.Linear, CompositeMode compositeMode = CompositeMode.SourceOver)
	{
		using Image image = effect.Output;
		DrawImage(image, targetOffset, null, interpolationMode, compositeMode);
	}

	public void DrawImage(Effect effect, InterpolationMode interpolationMode = InterpolationMode.Linear, CompositeMode compositeMode = CompositeMode.SourceOver)
	{
		using Image image = effect.Output;
		DrawImage(image, null, null, interpolationMode, compositeMode);
	}

	public void DrawImage(Image image, RawVector2 targetOffset, InterpolationMode interpolationMode = InterpolationMode.Linear, CompositeMode compositeMode = CompositeMode.SourceOver)
	{
		DrawImage(image, targetOffset, null, interpolationMode, compositeMode);
	}

	public void DrawImage(Image image, InterpolationMode interpolationMode = InterpolationMode.Linear, CompositeMode compositeMode = CompositeMode.SourceOver)
	{
		DrawImage(image, null, null, interpolationMode, compositeMode);
	}

	public void DrawBitmap(Bitmap bitmap, float opacity, InterpolationMode interpolationMode)
	{
		DrawBitmap(bitmap, null, opacity, interpolationMode, null, null);
	}

	public void DrawBitmap(Bitmap bitmap, float opacity, InterpolationMode interpolationMode, RawMatrix perspectiveTransformRef)
	{
		DrawBitmap(bitmap, null, opacity, interpolationMode, null, perspectiveTransformRef);
	}

	public void DrawBitmap(Bitmap bitmap, float opacity, InterpolationMode interpolationMode, RawRectangleF sourceRectangle, RawMatrix perspectiveTransformRef)
	{
		DrawBitmap(bitmap, null, opacity, interpolationMode, sourceRectangle, perspectiveTransformRef);
	}

	public void PushLayer(LayerParameters1 layerParameters, Layer layer)
	{
		PushLayer(ref layerParameters, layer);
	}

	public RawRectangleF[] GetEffectInvalidRectangles(Effect effect)
	{
		RawRectangleF[] array = new RawRectangleF[GetEffectInvalidRectangleCount(effect)];
		if (array.Length == 0)
		{
			return array;
		}
		GetEffectInvalidRectangles(effect, array, array.Length);
		return array;
	}

	public RawRectangleF[] GetEffectRequiredInputRectangles(Effect renderEffect, EffectInputDescription[] inputDescriptions)
	{
		RawRectangleF[] array = new RawRectangleF[inputDescriptions.Length];
		GetEffectRequiredInputRectangles(renderEffect, null, inputDescriptions, array, inputDescriptions.Length);
		return array;
	}

	public RawRectangleF[] GetEffectRequiredInputRectangles(Effect renderEffect, RawRectangleF renderImageRectangle, EffectInputDescription[] inputDescriptions)
	{
		RawRectangleF[] array = new RawRectangleF[inputDescriptions.Length];
		GetEffectRequiredInputRectangles(renderEffect, renderImageRectangle, inputDescriptions, array, inputDescriptions.Length);
		return array;
	}

	public void FillOpacityMask(Bitmap opacityMask, Brush brush)
	{
		FillOpacityMask(opacityMask, brush, null, null);
	}

	public DeviceContext(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateBitmap(Size2 size, IntPtr sourceData, int pitch, BitmapProperties1 bitmapProperties, Bitmap1 bitmap)
	{
		BitmapProperties1.__Native @ref = default(BitmapProperties1.__Native);
		IntPtr zero = IntPtr.Zero;
		bitmapProperties.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, Size2, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer, size, (void*)sourceData, pitch, &@ref, &zero);
		bitmap.NativePointer = zero;
		bitmapProperties.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromWicBitmap(BitmapSource wicBitmapSource, BitmapProperties1 bitmapProperties, out Bitmap1 bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		BitmapProperties1.__Native @ref = default(BitmapProperties1.__Native);
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(wicBitmapSource);
		bitmapProperties?.__MarshalTo(ref @ref);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)58 * (nint)sizeof(void*))))(nativePointer, intPtr, (bitmapProperties == null) ? null : (&@ref), &zero2);
		if (zero2 != IntPtr.Zero)
		{
			bitmap = new Bitmap1(zero2);
		}
		else
		{
			bitmap = null;
		}
		bitmapProperties?.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void CreateColorContext(ColorSpace space, byte[] rofileRef, int profileSize, ColorContext colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (byte* ptr = rofileRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)59 * (nint)sizeof(void*))))(_nativePointer, (int)space, ptr2, profileSize, &zero);
		}
		colorContext.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateColorContextFromFilename(string filename, ColorContext colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = filename)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)60 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		colorContext.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateColorContextFromWicColorContext(SharpDX.WIC.ColorContext wicColorContext, ColorContext colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.WIC.ColorContext>(wicColorContext);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)61 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		colorContext.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromDxgiSurface(Surface surface, BitmapProperties1 bitmapProperties, Bitmap1 bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		BitmapProperties1.__Native @ref = default(BitmapProperties1.__Native);
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Surface>(surface);
		bitmapProperties?.__MarshalTo(ref @ref);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)62 * (nint)sizeof(void*))))(nativePointer, intPtr, (bitmapProperties == null) ? null : (&@ref), &zero2);
		bitmap.NativePointer = zero2;
		bitmapProperties?.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void CreateEffect(Guid effectId, Effect effect)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)63 * (nint)sizeof(void*))))(_nativePointer, &effectId, &zero);
		effect.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGradientStopCollection(GradientStop[] straightAlphaGradientStops, int straightAlphaGradientStopsCount, ColorSpace preInterpolationSpace, ColorSpace postInterpolationSpace, BufferPrecision bufferPrecision, ExtendMode extendMode, ColorInterpolationMode colorInterpolationMode, GradientStopCollection1 gradientStopCollection1)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (GradientStop* ptr = straightAlphaGradientStops)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(_nativePointer, ptr2, straightAlphaGradientStopsCount, (int)preInterpolationSpace, (int)postInterpolationSpace, (int)bufferPrecision, (int)extendMode, (int)colorInterpolationMode, &zero);
		}
		gradientStopCollection1.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateImageBrush(Image image, ref ImageBrushProperties imageBrushProperties, BrushProperties? brushProperties, ImageBrush imageBrush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(image);
		BrushProperties value = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value = brushProperties.Value;
		}
		Result result;
		fixed (ImageBrushProperties* ptr = &imageBrushProperties)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			void* intPtr = (void*)zero;
			BrushProperties* intPtr2 = ((!brushProperties.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)65 * (nint)sizeof(void*))))(nativePointer, intPtr, ptr2, intPtr2, &zero2);
		}
		imageBrush.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateBitmapBrush(Bitmap bitmap, BitmapBrushProperties1? bitmapBrushProperties, BrushProperties? brushProperties, BitmapBrush1 bitmapBrush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Bitmap>(bitmap);
		BitmapBrushProperties1 value = default(BitmapBrushProperties1);
		if (bitmapBrushProperties.HasValue)
		{
			value = bitmapBrushProperties.Value;
		}
		BrushProperties value2 = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value2 = brushProperties.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		BitmapBrushProperties1* intPtr2 = ((!bitmapBrushProperties.HasValue) ? null : (&value));
		BrushProperties* intPtr3 = ((!brushProperties.HasValue) ? null : (&value2));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)66 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, &zero2);
		bitmapBrush.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateCommandList(CommandList commandList)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)67 * (nint)sizeof(void*))))(_nativePointer, &zero);
		commandList.NativePointer = zero;
		result.CheckError();
	}

	public unsafe RawBool IsDxgiFormatSupported(Format format)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)68 * (nint)sizeof(void*))))(_nativePointer, (int)format);
	}

	public unsafe RawBool IsBufferPrecisionSupported(BufferPrecision bufferPrecision)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)69 * (nint)sizeof(void*))))(_nativePointer, (int)bufferPrecision);
	}

	public unsafe RawRectangleF GetImageLocalBounds(Image image)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(image);
		RawRectangleF result = default(RawRectangleF);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)70 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &result)).CheckError();
		return result;
	}

	public unsafe RawRectangleF GetImageWorldBounds(Image image)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(image);
		RawRectangleF result = default(RawRectangleF);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)71 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &result)).CheckError();
		return result;
	}

	public unsafe RawRectangleF GetGlyphRunWorldBounds(RawVector2 baselineOrigin, GlyphRun glyphRun, MeasuringMode measuringMode)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		glyphRun.__MarshalTo(ref @ref);
		RawRectangleF result2 = default(RawRectangleF);
		Result result = ((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)72 * (nint)sizeof(void*))))(_nativePointer, baselineOrigin, &@ref, (int)measuringMode, &result2);
		glyphRun.__MarshalFree(ref @ref);
		result.CheckError();
		return result2;
	}

	internal unsafe void GetDevice(out Device device)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)73 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			device = new Device(zero);
		}
		else
		{
			device = null;
		}
	}

	internal unsafe void SetTarget(Image image)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(image);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)74 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe void GetTarget(out Image image)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)75 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			image = new Image(zero);
		}
		else
		{
			image = null;
		}
	}

	internal unsafe void SetRenderingControls(RenderingControls renderingControls)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)76 * (nint)sizeof(void*))))(_nativePointer, &renderingControls);
	}

	internal unsafe void GetRenderingControls(out RenderingControls renderingControls)
	{
		renderingControls = default(RenderingControls);
		fixed (RenderingControls* ptr = &renderingControls)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)77 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void SetPrimitiveBlend(PrimitiveBlend primitiveBlend)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)78 * (nint)sizeof(void*))))(_nativePointer, (int)primitiveBlend);
	}

	internal unsafe PrimitiveBlend GetPrimitiveBlend()
	{
		return ((delegate* unmanaged[Stdcall]<void*, PrimitiveBlend>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)79 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetUnitMode(UnitMode unitMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)80 * (nint)sizeof(void*))))(_nativePointer, (int)unitMode);
	}

	internal unsafe UnitMode GetUnitMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, UnitMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)81 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void DrawGlyphRun(RawVector2 baselineOrigin, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, Brush foregroundBrush, MeasuringMode measuringMode)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		GlyphRunDescription.__Native ref2 = default(GlyphRunDescription.__Native);
		IntPtr zero = IntPtr.Zero;
		glyphRun.__MarshalTo(ref @ref);
		glyphRunDescription?.__MarshalTo(ref ref2);
		zero = CppObject.ToCallbackPtr<Brush>(foregroundBrush);
		void* nativePointer = _nativePointer;
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)82 * (nint)sizeof(void*))))(nativePointer, baselineOrigin, &@ref, (glyphRunDescription == null) ? null : (&ref2), (void*)zero, (int)measuringMode);
		glyphRun.__MarshalFree(ref @ref);
		glyphRunDescription?.__MarshalFree(ref ref2);
	}

	public unsafe void DrawImage(Image image, RawVector2? targetOffset, RawRectangleF? imageRectangle, InterpolationMode interpolationMode, CompositeMode compositeMode)
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
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)83 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, (int)interpolationMode, (int)compositeMode);
	}

	public unsafe void DrawGdiMetafile(GdiMetafile gdiMetafile, RawVector2? targetOffset)
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
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)84 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2);
	}

	public unsafe void DrawBitmap(Bitmap bitmap, RawRectangleF? destinationRectangle, float opacity, InterpolationMode interpolationMode, RawRectangleF? sourceRectangle, RawMatrix? erspectiveTransformRef)
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
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)85 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, opacity, (int)interpolationMode, intPtr3, intPtr4);
	}

	public unsafe void PushLayer(ref LayerParameters1 layerParameters, Layer layer)
	{
		LayerParameters1.__Native @ref = default(LayerParameters1.__Native);
		IntPtr zero = IntPtr.Zero;
		layerParameters.__MarshalTo(ref @ref);
		zero = CppObject.ToCallbackPtr<Layer>(layer);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)86 * (nint)sizeof(void*))))(_nativePointer, &@ref, (void*)zero);
		layerParameters.__MarshalFree(ref @ref);
	}

	public unsafe void InvalidateEffectInputRectangle(Effect effect, int input, RawRectangleF inputRectangle)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Effect>(effect);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)87 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, input, &inputRectangle)).CheckError();
	}

	internal unsafe int GetEffectInvalidRectangleCount(Effect effect)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Effect>(effect);
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)88 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &result)).CheckError();
		return result;
	}

	internal unsafe void GetEffectInvalidRectangles(Effect effect, RawRectangleF[] rectangles, int rectanglesCount)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Effect>(effect);
		Result result;
		fixed (RawRectangleF* ptr = rectangles)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)89 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, rectanglesCount);
		}
		result.CheckError();
	}

	internal unsafe void GetEffectRequiredInputRectangles(Effect renderEffect, RawRectangleF? renderImageRectangle, EffectInputDescription[] inputDescriptions, RawRectangleF[] requiredInputRects, int inputCount)
	{
		IntPtr zero = IntPtr.Zero;
		EffectInputDescription.__Native[] array = new EffectInputDescription.__Native[inputDescriptions.Length];
		zero = CppObject.ToCallbackPtr<Effect>(renderEffect);
		RawRectangleF value = default(RawRectangleF);
		if (renderImageRectangle.HasValue)
		{
			value = renderImageRectangle.Value;
		}
		for (int i = 0; i < inputDescriptions.Length; i++)
		{
			inputDescriptions[i].__MarshalTo(ref array[i]);
		}
		Result result;
		fixed (RawRectangleF* ptr = requiredInputRects)
		{
			void* ptr2 = ptr;
			fixed (EffectInputDescription.__Native* ptr3 = array)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				void* intPtr = (void*)zero;
				RawRectangleF* intPtr2 = ((!renderImageRectangle.HasValue) ? null : (&value));
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)90 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, ptr4, ptr2, inputCount);
			}
		}
		for (int j = 0; j < inputDescriptions.Length; j++)
		{
			inputDescriptions[j].__MarshalFree(ref array[j]);
		}
		result.CheckError();
	}

	public unsafe void FillOpacityMask(Bitmap opacityMask, Brush brush, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle)
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
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)91 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, intPtr4);
	}
}

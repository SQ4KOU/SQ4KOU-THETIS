using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("2cd90694-12e2-11dc-9fed-001143a055f9")]
public class RenderTarget : Resource
{
	private float _strokeWidth = 1f;

	public const float DefaultStrokeWidth = 1f;

	public float StrokeWidth
	{
		get
		{
			return _strokeWidth;
		}
		set
		{
			_strokeWidth = value;
		}
	}

	public Size2F DotsPerInch
	{
		get
		{
			GetDpi(out var dpiX, out var dpiY);
			return new Size2F(dpiX, dpiY);
		}
		set
		{
			SetDpi(value.Width, value.Height);
		}
	}

	public RawMatrix3x2 Transform
	{
		get
		{
			GetTransform(out var transform);
			return transform;
		}
		set
		{
			SetTransform(ref value);
		}
	}

	public AntialiasMode AntialiasMode
	{
		get
		{
			return GetAntialiasMode();
		}
		set
		{
			SetAntialiasMode(value);
		}
	}

	public TextAntialiasMode TextAntialiasMode
	{
		get
		{
			return GetTextAntialiasMode();
		}
		set
		{
			SetTextAntialiasMode(value);
		}
	}

	public RenderingParams TextRenderingParams
	{
		get
		{
			GetTextRenderingParams(out var textRenderingParams);
			return textRenderingParams;
		}
		set
		{
			SetTextRenderingParams(value);
		}
	}

	public PixelFormat PixelFormat => GetPixelFormat();

	public Size2F Size => GetSize();

	public Size2 PixelSize => GetPixelSize();

	public int MaximumBitmapSize => GetMaximumBitmapSize();

	public RenderTarget(Factory factory, Surface dxgiSurface, RenderTargetProperties properties)
		: base(IntPtr.Zero)
	{
		factory.CreateDxgiSurfaceRenderTarget(dxgiSurface, ref properties, this);
	}

	public void DrawBitmap(Bitmap bitmap, float opacity, BitmapInterpolationMode interpolationMode)
	{
		DrawBitmap(bitmap, null, opacity, interpolationMode, null);
	}

	public void DrawBitmap(Bitmap bitmap, RawRectangleF destinationRectangle, float opacity, BitmapInterpolationMode interpolationMode)
	{
		DrawBitmap(bitmap, destinationRectangle, opacity, interpolationMode, null);
	}

	public void DrawBitmap(Bitmap bitmap, float opacity, BitmapInterpolationMode interpolationMode, RawRectangleF sourceRectangle)
	{
		DrawBitmap(bitmap, null, opacity, interpolationMode, sourceRectangle);
	}

	public void DrawEllipse(Ellipse ellipse, Brush brush)
	{
		DrawEllipse(ellipse, brush, StrokeWidth, null);
	}

	public void DrawEllipse(Ellipse ellipse, Brush brush, float strokeWidth)
	{
		DrawEllipse(ellipse, brush, strokeWidth, null);
	}

	public void DrawGeometry(Geometry geometry, Brush brush)
	{
		DrawGeometry(geometry, brush, StrokeWidth, null);
	}

	public void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth)
	{
		DrawGeometry(geometry, brush, strokeWidth, null);
	}

	public void DrawLine(RawVector2 point0, RawVector2 point1, Brush brush)
	{
		DrawLine(point0, point1, brush, StrokeWidth, null);
	}

	public void DrawLine(RawVector2 point0, RawVector2 point1, Brush brush, float strokeWidth)
	{
		DrawLine(point0, point1, brush, strokeWidth, null);
	}

	public void DrawRectangle(RawRectangleF rect, Brush brush)
	{
		DrawRectangle(rect, brush, StrokeWidth, null);
	}

	public void DrawRectangle(RawRectangleF rect, Brush brush, float strokeWidth)
	{
		DrawRectangle(rect, brush, strokeWidth, null);
	}

	public void DrawRoundedRectangle(RoundedRectangle roundedRect, Brush brush)
	{
		DrawRoundedRectangle(ref roundedRect, brush, StrokeWidth, null);
	}

	public void DrawRoundedRectangle(RoundedRectangle roundedRect, Brush brush, float strokeWidth)
	{
		DrawRoundedRectangle(ref roundedRect, brush, strokeWidth, null);
	}

	public void DrawRoundedRectangle(RoundedRectangle roundedRect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		DrawRoundedRectangle(ref roundedRect, brush, strokeWidth, strokeStyle);
	}

	public void DrawText(string text, TextFormat textFormat, RawRectangleF layoutRect, Brush defaultForegroundBrush)
	{
		DrawText(text, text.Length, textFormat, layoutRect, defaultForegroundBrush, DrawTextOptions.None, MeasuringMode.Natural);
	}

	public void DrawText(string text, TextFormat textFormat, RawRectangleF layoutRect, Brush defaultForegroundBrush, DrawTextOptions options)
	{
		DrawText(text, text.Length, textFormat, layoutRect, defaultForegroundBrush, options, MeasuringMode.Natural);
	}

	public void DrawText(string text, TextFormat textFormat, RawRectangleF layoutRect, Brush defaultForegroundBrush, DrawTextOptions options, MeasuringMode measuringMode)
	{
		DrawText(text, text.Length, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode);
	}

	public void DrawTextLayout(RawVector2 origin, TextLayout textLayout, Brush defaultForegroundBrush)
	{
		DrawTextLayout(origin, textLayout, defaultForegroundBrush, DrawTextOptions.None);
	}

	public void EndDraw(out long tag1, out long tag2)
	{
		TryEndDraw(out tag1, out tag2).CheckError();
	}

	public void EndDraw()
	{
		EndDraw(out var _, out var _);
	}

	public void FillGeometry(Geometry geometry, Brush brush)
	{
		FillGeometry(geometry, brush, null);
	}

	public void FillOpacityMask(Bitmap opacityMask, Brush brush, OpacityMaskContent content)
	{
		FillOpacityMask(opacityMask, brush, content, null, null);
	}

	public void FillRoundedRectangle(RoundedRectangle roundedRect, Brush brush)
	{
		FillRoundedRectangle(ref roundedRect, brush);
	}

	public void Flush()
	{
		Flush(out var _, out var _);
	}

	public RenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderTarget(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RenderTarget(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateBitmap(Size2 size, IntPtr srcData, int pitch, BitmapProperties bitmapProperties, Bitmap bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, Size2, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, size, (void*)srcData, pitch, &bitmapProperties, &zero);
		bitmap.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromWicBitmap(BitmapSource wicBitmapSource, BitmapProperties? bitmapProperties, out Bitmap bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(wicBitmapSource);
		BitmapProperties value = default(BitmapProperties);
		if (bitmapProperties.HasValue)
		{
			value = bitmapProperties.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		BitmapProperties* intPtr2 = ((!bitmapProperties.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			bitmap = new Bitmap(zero2);
		}
		else
		{
			bitmap = null;
		}
		result.CheckError();
	}

	internal unsafe void CreateSharedBitmap(Guid riid, IntPtr data, BitmapProperties? bitmapProperties, Bitmap bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		BitmapProperties value = default(BitmapProperties);
		if (bitmapProperties.HasValue)
		{
			value = bitmapProperties.Value;
		}
		void* nativePointer = _nativePointer;
		Guid* num = &riid;
		void* intPtr = (void*)data;
		BitmapProperties* intPtr2 = ((!bitmapProperties.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, num, intPtr, intPtr2, &zero);
		bitmap.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapBrush(Bitmap bitmap, BitmapBrushProperties? bitmapBrushProperties, BrushProperties? brushProperties, BitmapBrush bitmapBrush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Bitmap>(bitmap);
		BitmapBrushProperties value = default(BitmapBrushProperties);
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
		BitmapBrushProperties* intPtr2 = ((!bitmapBrushProperties.HasValue) ? null : (&value));
		BrushProperties* intPtr3 = ((!brushProperties.HasValue) ? null : (&value2));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, &zero2);
		bitmapBrush.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateSolidColorBrush(RawColor4 color, BrushProperties? brushProperties, SolidColorBrush solidColorBrush)
	{
		IntPtr zero = IntPtr.Zero;
		BrushProperties value = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value = brushProperties.Value;
		}
		void* nativePointer = _nativePointer;
		RawColor4* num = &color;
		BrushProperties* intPtr = ((!brushProperties.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, num, intPtr, &zero);
		solidColorBrush.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGradientStopCollection(GradientStop[] gradientStops, int gradientStopsCount, Gamma colorInterpolationGamma, ExtendMode extendMode, GradientStopCollection gradientStopCollection)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (GradientStop* ptr = gradientStops)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2, gradientStopsCount, (int)colorInterpolationGamma, (int)extendMode, &zero);
		}
		gradientStopCollection.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateLinearGradientBrush(LinearGradientBrushProperties linearGradientBrushProperties, BrushProperties? brushProperties, GradientStopCollection gradientStopCollection, LinearGradientBrush linearGradientBrush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		BrushProperties value = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value = brushProperties.Value;
		}
		zero = CppObject.ToCallbackPtr<GradientStopCollection>(gradientStopCollection);
		void* nativePointer = _nativePointer;
		LinearGradientBrushProperties* num = &linearGradientBrushProperties;
		BrushProperties* intPtr = ((!brushProperties.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, num, intPtr, (void*)zero, &zero2);
		linearGradientBrush.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateRadialGradientBrush(ref RadialGradientBrushProperties radialGradientBrushProperties, BrushProperties? brushProperties, GradientStopCollection gradientStopCollection, RadialGradientBrush radialGradientBrush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		BrushProperties value = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value = brushProperties.Value;
		}
		zero = CppObject.ToCallbackPtr<GradientStopCollection>(gradientStopCollection);
		Result result;
		fixed (RadialGradientBrushProperties* ptr = &radialGradientBrushProperties)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			BrushProperties* intPtr = ((!brushProperties.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, ptr2, intPtr, (void*)zero, &zero2);
		}
		radialGradientBrush.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateCompatibleRenderTarget(Size2F? desiredSize, Size2? desiredPixelSize, PixelFormat? desiredFormat, CompatibleRenderTargetOptions options, BitmapRenderTarget bitmapRenderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		Size2F value = default(Size2F);
		if (desiredSize.HasValue)
		{
			value = desiredSize.Value;
		}
		Size2 value2 = default(Size2);
		if (desiredPixelSize.HasValue)
		{
			value2 = desiredPixelSize.Value;
		}
		PixelFormat value3 = default(PixelFormat);
		if (desiredFormat.HasValue)
		{
			value3 = desiredFormat.Value;
		}
		void* nativePointer = _nativePointer;
		Size2F* intPtr = ((!desiredSize.HasValue) ? null : (&value));
		Size2* intPtr2 = ((!desiredPixelSize.HasValue) ? null : (&value2));
		PixelFormat* intPtr3 = ((!desiredFormat.HasValue) ? null : (&value3));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, (int)options, &zero);
		bitmapRenderTarget.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateLayer(Size2F? size, Layer layer)
	{
		IntPtr zero = IntPtr.Zero;
		Size2F value = default(Size2F);
		if (size.HasValue)
		{
			value = size.Value;
		}
		void* nativePointer = _nativePointer;
		Size2F* intPtr = ((!size.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, intPtr, &zero);
		layer.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateMesh(Mesh mesh)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, &zero);
		mesh.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void DrawLine(RawVector2 point0, RawVector2 point1, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		zero2 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		((delegate* unmanaged[Stdcall]<void*, RawVector2, RawVector2, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, point0, point1, (void*)zero, strokeWidth, (void*)zero2);
	}

	public unsafe void DrawRectangle(RawRectangleF rect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		zero2 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, &rect, (void*)zero, strokeWidth, (void*)zero2);
	}

	public unsafe void FillRectangle(RawRectangleF rect, Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, &rect, (void*)zero);
	}

	public unsafe void DrawRoundedRectangle(ref RoundedRectangle roundedRect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		zero2 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		fixed (RoundedRectangle* ptr = &roundedRect)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr2, (void*)zero, strokeWidth, (void*)zero2);
		}
	}

	public unsafe void FillRoundedRectangle(ref RoundedRectangle roundedRect, Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		fixed (RoundedRectangle* ptr = &roundedRect)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr2, (void*)zero);
		}
	}

	public unsafe void DrawEllipse(Ellipse ellipse, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		zero2 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, &ellipse, (void*)zero, strokeWidth, (void*)zero2);
	}

	public unsafe void FillEllipse(Ellipse ellipse, Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, &ellipse, (void*)zero);
	}

	public unsafe void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(geometry);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		zero3 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, strokeWidth, (void*)zero3);
	}

	public unsafe void FillGeometry(Geometry geometry, Brush brush, Brush opacityBrush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(geometry);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		zero3 = CppObject.ToCallbackPtr<Brush>(opacityBrush);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, (void*)zero3);
	}

	public unsafe void FillMesh(Mesh mesh, Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Mesh>(mesh);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2);
	}

	public unsafe void FillOpacityMask(Bitmap opacityMask, Brush brush, OpacityMaskContent content, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle)
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
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, (int)content, intPtr3, intPtr4);
	}

	public unsafe void DrawBitmap(Bitmap bitmap, RawRectangleF? destinationRectangle, float opacity, BitmapInterpolationMode interpolationMode, RawRectangleF? sourceRectangle)
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
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawRectangleF* intPtr2 = ((!destinationRectangle.HasValue) ? null : (&value));
		RawRectangleF* intPtr3 = ((!sourceRectangle.HasValue) ? null : (&value2));
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, opacity, (int)interpolationMode, intPtr3);
	}

	public unsafe void DrawText(string text, int stringLength, TextFormat textFormat, RawRectangleF layoutRect, Brush defaultFillBrush, DrawTextOptions options, MeasuringMode measuringMode)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextFormat>(textFormat);
		zero2 = CppObject.ToCallbackPtr<Brush>(defaultFillBrush);
		fixed (char* ptr = text)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, ptr, stringLength, (void*)zero, &layoutRect, (void*)zero2, (int)options, (int)measuringMode);
		}
	}

	public unsafe void DrawTextLayout(RawVector2 origin, TextLayout textLayout, Brush defaultFillBrush, DrawTextOptions options)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextLayout>(textLayout);
		zero2 = CppObject.ToCallbackPtr<Brush>(defaultFillBrush);
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, origin, (void*)zero, (void*)zero2, (int)options);
	}

	public unsafe void DrawGlyphRun(RawVector2 baselineOrigin, GlyphRun glyphRun, Brush foregroundBrush, MeasuringMode measuringMode)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		IntPtr zero = IntPtr.Zero;
		glyphRun.__MarshalTo(ref @ref);
		zero = CppObject.ToCallbackPtr<Brush>(foregroundBrush);
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, baselineOrigin, &@ref, (void*)zero, (int)measuringMode);
		glyphRun.__MarshalFree(ref @ref);
	}

	internal unsafe void SetTransform(ref RawMatrix3x2 transform)
	{
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetTransform(out RawMatrix3x2 transform)
	{
		transform = default(RawMatrix3x2);
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void SetAntialiasMode(AntialiasMode antialiasMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (int)antialiasMode);
	}

	internal unsafe AntialiasMode GetAntialiasMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, AntialiasMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetTextAntialiasMode(TextAntialiasMode textAntialiasMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, (int)textAntialiasMode);
	}

	internal unsafe TextAntialiasMode GetTextAntialiasMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, TextAntialiasMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetTextRenderingParams(RenderingParams textRenderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<RenderingParams>(textRenderingParams);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe void GetTextRenderingParams(out RenderingParams textRenderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			textRenderingParams = new RenderingParams(zero);
		}
		else
		{
			textRenderingParams = null;
		}
	}

	public unsafe void SetTags(long tag1, long tag2)
	{
		((delegate* unmanaged[Stdcall]<void*, long, long, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, tag1, tag2);
	}

	public unsafe void GetTags(out long tag1, out long tag2)
	{
		fixed (long* ptr = &tag2)
		{
			void* ptr2 = ptr;
			fixed (long* ptr3 = &tag1)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
	}

	public unsafe void PushLayer(ref LayerParameters layerParameters, Layer layer)
	{
		LayerParameters.__Native @ref = default(LayerParameters.__Native);
		IntPtr zero = IntPtr.Zero;
		layerParameters.__MarshalTo(ref @ref);
		zero = CppObject.ToCallbackPtr<Layer>(layer);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, &@ref, (void*)zero);
		layerParameters.__MarshalFree(ref @ref);
	}

	public unsafe void PopLayer()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Flush(out long tag1, out long tag2)
	{
		Result result;
		fixed (long* ptr = &tag2)
		{
			void* ptr2 = ptr;
			fixed (long* ptr3 = &tag1)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void SaveDrawingState(DrawingStateBlock drawingStateBlock)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DrawingStateBlock>(drawingStateBlock);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void RestoreDrawingState(DrawingStateBlock drawingStateBlock)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DrawingStateBlock>(drawingStateBlock);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void PushAxisAlignedClip(RawRectangleF clipRect, AntialiasMode antialiasMode)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, &clipRect, (int)antialiasMode);
	}

	public unsafe void PopAxisAlignedClip()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Clear(RawColor4? clearColor)
	{
		RawColor4 value = default(RawColor4);
		if (clearColor.HasValue)
		{
			value = clearColor.Value;
		}
		void* nativePointer = _nativePointer;
		RawColor4* intPtr = ((!clearColor.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(nativePointer, intPtr);
	}

	public unsafe void BeginDraw()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe Result TryEndDraw(out long tag1, out long tag2)
	{
		Result result;
		fixed (long* ptr = &tag2)
		{
			void* ptr2 = ptr;
			fixed (long* ptr3 = &tag1)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		return result;
	}

	internal unsafe PixelFormat GetPixelFormat()
	{
		PixelFormat result = default(PixelFormat);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe void SetDpi(float dpiX, float dpiY)
	{
		((delegate* unmanaged[Stdcall]<void*, float, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, dpiX, dpiY);
	}

	internal unsafe void GetDpi(out float dpiX, out float dpiY)
	{
		fixed (float* ptr = &dpiY)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &dpiX)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
	}

	internal unsafe Size2F GetSize()
	{
		Size2F result = default(Size2F);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)53 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe Size2 GetPixelSize()
	{
		Size2 result = default(Size2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)54 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe int GetMaximumBitmapSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)55 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe RawBool IsSupported(ref RenderTargetProperties renderTargetProperties)
	{
		RawBool result;
		fixed (RenderTargetProperties* ptr = &renderTargetProperties)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)56 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		return result;
	}
}

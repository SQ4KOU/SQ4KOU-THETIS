using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("00000105-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapFrameEncode : ComObject
{
	public BitmapEncoderOptions Options { get; private set; }

	public Palette Palette
	{
		set
		{
			SetPalette(value);
		}
	}

	public BitmapSource Thumbnail
	{
		set
		{
			SetThumbnail(value);
		}
	}

	public MetadataQueryWriter MetadataQueryWriter
	{
		get
		{
			GetMetadataQueryWriter(out var metadataQueryWriterOut);
			return metadataQueryWriterOut;
		}
	}

	public BitmapFrameEncode(BitmapEncoder encoder)
	{
		Options = new BitmapEncoderOptions(IntPtr.Zero);
		encoder.CreateNewFrame(this, Options);
	}

	public void Initialize()
	{
		Initialize(Options);
	}

	public void SetColorContexts(ColorContext[] colorContextOut)
	{
		SetColorContexts((colorContextOut != null) ? colorContextOut.Length : 0, colorContextOut);
	}

	public void WritePixels(int lineCount, DataRectangle buffer, int totalSizeInBytes = 0)
	{
		WritePixels(lineCount, buffer.DataPointer, buffer.Pitch, totalSizeInBytes);
	}

	public void WritePixels(int lineCount, IntPtr buffer, int rowStride, int totalSizeInBytes = 0)
	{
		if (totalSizeInBytes == 0)
		{
			totalSizeInBytes = lineCount * rowStride;
		}
		WritePixels(lineCount, rowStride, totalSizeInBytes, buffer);
	}

	public unsafe void WritePixels<T>(int lineCount, int stride, T[] pixelBuffer) where T : struct
	{
		if (lineCount * stride > Utilities.SizeOf<T>() * pixelBuffer.Length)
		{
			throw new ArgumentException("lineCount * stride must be <= to sizeof(pixelBuffer)");
		}
		int bufferSize = lineCount * stride;
		fixed (T* ptr = &pixelBuffer[0])
		{
			WritePixels(lineCount, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public void WriteSource(BitmapSource bitmapSource)
	{
		WriteSource(bitmapSource, IntPtr.Zero);
	}

	public unsafe void WriteSource(BitmapSource bitmapSourceRef, RawBox rectangleRef)
	{
		WriteSource(bitmapSourceRef, new IntPtr(&rectangleRef));
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Options.Dispose();
			Options = null;
		}
		base.Dispose(disposing);
	}

	public BitmapFrameEncode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapFrameEncode(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapFrameEncode(nativePtr);
		}
		return null;
	}

	internal unsafe void Initialize(PropertyBag encoderOptionsRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<PropertyBag>(encoderOptionsRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void SetSize(int width, int height)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, width, height)).CheckError();
	}

	public unsafe void SetResolution(double dpiX, double dpiY)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, dpiX, dpiY)).CheckError();
	}

	public unsafe void SetPixelFormat(ref Guid pixelFormatRef)
	{
		Result result;
		fixed (Guid* ptr = &pixelFormatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void SetColorContexts(int count, ColorContext[] colorContextOut)
	{
		IntPtr* ptr = null;
		if (colorContextOut != null)
		{
			ptr = stackalloc IntPtr[colorContextOut.Length];
		}
		if (colorContextOut != null)
		{
			for (int i = 0; i < colorContextOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<ColorContext>(colorContextOut[i]);
			}
		}
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, count, ptr)).CheckError();
	}

	internal unsafe void SetPalette(Palette paletteRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Palette>(paletteRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void SetThumbnail(BitmapSource thumbnailRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(thumbnailRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void WritePixels(int lineCount, int stride, int bufferSize, IntPtr pixelsRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, lineCount, stride, bufferSize, (void*)pixelsRef)).CheckError();
	}

	internal unsafe void WriteSource(BitmapSource bitmapSourceRef, IntPtr rectangleRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(bitmapSourceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)rectangleRef)).CheckError();
	}

	public unsafe void Commit()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void GetMetadataQueryWriter(out MetadataQueryWriter metadataQueryWriterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			metadataQueryWriterOut = new MetadataQueryWriter(zero);
		}
		else
		{
			metadataQueryWriterOut = null;
		}
		result.CheckError();
	}

	internal unsafe void SetColorContexts(int count, ComArray<ColorContext> colorContextOut)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = colorContextOut?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, count, (void*)intPtr)).CheckError();
	}

	private unsafe void SetColorContexts(int count, IntPtr colorContextOut)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, count, (void*)colorContextOut)).CheckError();
	}
}

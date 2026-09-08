using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("00000120-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapSource : ComObject
{
	public Size2 Size
	{
		get
		{
			GetSize(out var widthRef, out var heightRef);
			return new Size2(widthRef, heightRef);
		}
	}

	public Guid PixelFormat
	{
		get
		{
			GetPixelFormat(out var pixelFormatRef);
			return pixelFormatRef;
		}
	}

	public unsafe void CopyPixels(RawBox rectangle, int stride, DataPointer dataPointer)
	{
		CopyPixels(new IntPtr(&rectangle), stride, dataPointer.Size, dataPointer.Pointer);
	}

	public void CopyPixels(int stride, DataPointer dataPointer)
	{
		CopyPixels(IntPtr.Zero, stride, dataPointer.Size, dataPointer.Pointer);
	}

	public void CopyPixels(int stride, IntPtr dataPointer, int size)
	{
		CopyPixels(IntPtr.Zero, stride, size, dataPointer);
	}

	public unsafe void CopyPixels<T>(RawBox rectangle, T[] output) where T : struct
	{
		if (rectangle.Width * rectangle.Height != output.Length)
		{
			throw new ArgumentException("output.Length must be equal to Width * Height");
		}
		IntPtr rectangleRef = new IntPtr(&rectangle);
		int stride = rectangle.Width * Utilities.SizeOf<T>();
		int bufferSize = output.Length * Utilities.SizeOf<T>();
		fixed (T* ptr = &output[0])
		{
			CopyPixels(rectangleRef, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public unsafe void CopyPixels<T>(T[] output) where T : struct
	{
		Size2 size = Size;
		if (size.Width * size.Height != output.Length)
		{
			throw new ArgumentException("output.Length must be equal to Width * Height");
		}
		IntPtr zero = IntPtr.Zero;
		int stride = Size.Width * Utilities.SizeOf<T>();
		int bufferSize = output.Length * Utilities.SizeOf<T>();
		fixed (T* ptr = &output[0])
		{
			CopyPixels(zero, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public unsafe void CopyPixels(RawRectangle rectangle, byte[] output, int stride)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		if (stride <= 0)
		{
			throw new ArgumentOutOfRangeException("stride", "Must be > 0");
		}
		if (output.Length % stride != 0)
		{
			throw new ArgumentException("output.Length must be a modulo of stride");
		}
		IntPtr rectangleRef = new IntPtr(&rectangle);
		int bufferSize = output.Length;
		fixed (byte* ptr = &output[0])
		{
			CopyPixels(rectangleRef, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public unsafe void CopyPixels(byte[] output, int stride)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		if (stride <= 0)
		{
			throw new ArgumentOutOfRangeException("stride", "Must be > 0");
		}
		if (output.Length % stride != 0)
		{
			throw new ArgumentException("output.Length must be a modulo of stride");
		}
		IntPtr zero = IntPtr.Zero;
		int bufferSize = output.Length;
		fixed (byte* ptr = &output[0])
		{
			CopyPixels(zero, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public BitmapSource(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapSource(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapSource(nativePtr);
		}
		return null;
	}

	internal unsafe void GetSize(out int widthRef, out int heightRef)
	{
		Result result;
		fixed (int* ptr = &heightRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &widthRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetPixelFormat(out Guid pixelFormatRef)
	{
		pixelFormatRef = default(Guid);
		Result result;
		fixed (Guid* ptr = &pixelFormatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetResolution(out double dpiXRef, out double dpiYRef)
	{
		Result result;
		fixed (double* ptr = &dpiYRef)
		{
			void* ptr2 = ptr;
			fixed (double* ptr3 = &dpiXRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void CopyPalette(Palette paletteRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Palette>(paletteRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void CopyPixels(IntPtr rectangleRef, int stride, int bufferSize, IntPtr bufferRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)rectangleRef, stride, bufferSize, (void*)bufferRef)).CheckError();
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("3B16811B-6A43-4ec9-A813-3D930C13B940")]
public class BitmapFrameDecode : BitmapSource
{
	[Obsolete("Use TryGetColorContexts instead")]
	public ColorContext[] ColorContexts => new ColorContext[0];

	public MetadataQueryReader MetadataQueryReader
	{
		get
		{
			GetMetadataQueryReader(out var metadataQueryReaderOut);
			return metadataQueryReaderOut;
		}
	}

	public BitmapSource Thumbnail
	{
		get
		{
			GetThumbnail(out var thumbnailOut);
			return thumbnailOut;
		}
	}

	public Result TryGetColorContexts(ImagingFactory imagingFactory, out ColorContext[] colorContexts)
	{
		return ColorContextsHelper.TryGetColorContexts(GetColorContexts, imagingFactory, out colorContexts);
	}

	public ColorContext[] TryGetColorContexts(ImagingFactory imagingFactory)
	{
		return ColorContextsHelper.TryGetColorContexts(GetColorContexts, imagingFactory);
	}

	public BitmapFrameDecode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapFrameDecode(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapFrameDecode(nativePtr);
		}
		return null;
	}

	internal unsafe void GetMetadataQueryReader(out MetadataQueryReader metadataQueryReaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			metadataQueryReaderOut = new MetadataQueryReader(zero);
		}
		else
		{
			metadataQueryReaderOut = null;
		}
		result.CheckError();
	}

	internal unsafe Result GetColorContexts(int count, ColorContext[] colorContextsOut, out int actualCountRef)
	{
		IntPtr* ptr = null;
		if (colorContextsOut != null)
		{
			ptr = stackalloc IntPtr[colorContextsOut.Length];
		}
		if (colorContextsOut != null)
		{
			for (int i = 0; i < colorContextsOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<ColorContext>(colorContextsOut[i]);
			}
		}
		Result result;
		fixed (int* ptr2 = &actualCountRef)
		{
			void* ptr3 = ptr2;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, count, ptr, ptr3);
		}
		return result;
	}

	internal unsafe void GetThumbnail(out BitmapSource thumbnailOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			thumbnailOut = new BitmapSource(zero);
		}
		else
		{
			thumbnailOut = null;
		}
		result.CheckError();
	}

	internal unsafe Result GetColorContexts(int count, ComArray<ColorContext> colorContextsOut, out int actualCountRef)
	{
		Result result;
		fixed (int* ptr = &actualCountRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			IntPtr intPtr = colorContextsOut?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, count, (void*)intPtr, ptr2);
		}
		return result;
	}

	private unsafe Result GetColorContexts(int count, IntPtr colorContextsOut, IntPtr actualCountRef)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, count, (void*)colorContextsOut, (void*)actualCountRef);
	}
}

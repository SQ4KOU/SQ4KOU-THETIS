using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.IO;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("9EDDE9E7-8DEE-47ea-99DF-E6FAF2ED44BF")]
public class BitmapDecoder : ComObject
{
	private WICStream internalWICStream;

	[Obsolete("Use TryGetColorContexts instead")]
	public ColorContext[] ColorContexts => new ColorContext[0];

	public Guid ContainerFormat
	{
		get
		{
			GetContainerFormat(out var guidContainerFormatRef);
			return guidContainerFormatRef;
		}
	}

	public BitmapDecoderInfo DecoderInfo
	{
		get
		{
			GetDecoderInfo(out var decoderInfoOut);
			return decoderInfoOut;
		}
	}

	public MetadataQueryReader MetadataQueryReader
	{
		get
		{
			GetMetadataQueryReader(out var metadataQueryReaderOut);
			return metadataQueryReaderOut;
		}
	}

	public BitmapSource Preview
	{
		get
		{
			GetPreview(out var bitmapSourceOut);
			return bitmapSourceOut;
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

	public int FrameCount
	{
		get
		{
			GetFrameCount(out var countRef);
			return countRef;
		}
	}

	public BitmapDecoder(BitmapDecoderInfo bitmapDecoderInfo)
	{
		bitmapDecoderInfo.CreateInstance(this);
	}

	public BitmapDecoder(ImagingFactory factory, Guid containerFormatGuid)
	{
		factory.CreateDecoder(containerFormatGuid, null, this);
	}

	public BitmapDecoder(ImagingFactory factory, Guid containerFormatGuid, Guid guidVendorRef)
	{
		factory.CreateDecoder(containerFormatGuid, guidVendorRef, this);
	}

	public BitmapDecoder(ImagingFactory factory, IStream streamRef, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromStream(streamRef, null, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, Stream streamRef, DecodeOptions metadataOptions)
	{
		internalWICStream = new WICStream(factory, streamRef);
		factory.CreateDecoderFromStream(internalWICStream, null, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, IStream streamRef, Guid guidVendorRef, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromStream(streamRef, guidVendorRef, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, Stream streamRef, Guid guidVendorRef, DecodeOptions metadataOptions)
	{
		internalWICStream = new WICStream(factory, streamRef);
		factory.CreateDecoderFromStream(internalWICStream, guidVendorRef, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, string filename, DecodeOptions metadataOptions)
		: this(factory, filename, null, NativeFileAccess.Read, metadataOptions)
	{
	}

	public BitmapDecoder(ImagingFactory factory, string filename, NativeFileAccess desiredAccess, DecodeOptions metadataOptions)
		: this(factory, filename, null, desiredAccess, metadataOptions)
	{
	}

	public BitmapDecoder(ImagingFactory factory, string filename, Guid? guidVendorRef, NativeFileAccess desiredAccess, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromFilename(filename, guidVendorRef, (int)desiredAccess, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, NativeFileStream fileStream, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromFileHandle(fileStream.Handle, null, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, NativeFileStream fileStream, Guid guidVendorRef, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromFileHandle(fileStream.Handle, guidVendorRef, metadataOptions, this);
	}

	public void Initialize(IStream stream, DecodeOptions cacheOptions)
	{
		if (internalWICStream != null)
		{
			throw new InvalidOperationException("This instance is already initialized with an existing stream");
		}
		Initialize_(stream, cacheOptions);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			if (internalWICStream != null)
			{
				internalWICStream.Dispose();
			}
			internalWICStream = null;
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

	public BitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapDecoder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapDecoder(nativePtr);
		}
		return null;
	}

	public unsafe BitmapDecoderCapabilities QueryCapability(IStream streamRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(streamRef);
		BitmapDecoderCapabilities result = default(BitmapDecoderCapabilities);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &result)).CheckError();
		return result;
	}

	internal unsafe void Initialize_(IStream streamRef, DecodeOptions cacheOptions)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(streamRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)cacheOptions)).CheckError();
	}

	internal unsafe void GetContainerFormat(out Guid guidContainerFormatRef)
	{
		guidContainerFormatRef = default(Guid);
		Result result;
		fixed (Guid* ptr = &guidContainerFormatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetDecoderInfo(out BitmapDecoderInfo decoderInfoOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			decoderInfoOut = new BitmapDecoderInfo(zero);
		}
		else
		{
			decoderInfoOut = null;
		}
		result.CheckError();
	}

	public unsafe void CopyPalette(Palette paletteRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Palette>(paletteRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
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

	internal unsafe void GetPreview(out BitmapSource bitmapSourceOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			bitmapSourceOut = new BitmapSource(zero);
		}
		else
		{
			bitmapSourceOut = null;
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
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, count, ptr, ptr3);
		}
		return result;
	}

	internal unsafe void GetThumbnail(out BitmapSource thumbnailOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &zero);
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

	internal unsafe void GetFrameCount(out int countRef)
	{
		Result result;
		fixed (int* ptr = &countRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe BitmapFrameDecode GetFrame(int index)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		BitmapFrameDecode result2 = ((!(zero != IntPtr.Zero)) ? null : new BitmapFrameDecode(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe Result GetColorContexts(int count, ComArray<ColorContext> colorContextsOut, out int actualCountRef)
	{
		Result result;
		fixed (int* ptr = &actualCountRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			IntPtr intPtr = colorContextsOut?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, count, (void*)intPtr, ptr2);
		}
		return result;
	}

	private unsafe Result GetColorContexts(int count, IntPtr colorContextsOut, IntPtr actualCountRef)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, count, (void*)colorContextsOut, (void*)actualCountRef);
	}
}

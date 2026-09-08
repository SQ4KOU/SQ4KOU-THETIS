using System;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("ec5ec8a9-c395-4314-9c77-54d7a935ff70")]
public class ImagingFactory : ComObject
{
	public static readonly Guid WICImagingFactoryClsid = new Guid("cacaf262-9370-4615-a13b-9f5539da4c0a");

	public ImagingFactory()
	{
		Utilities.CreateComInstance(WICImagingFactoryClsid, Utilities.CLSCTX.ClsctxInprocServer, Utilities.GetGuidFromType(typeof(ImagingFactory)), this);
	}

	public ImagingFactory(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ImagingFactory(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ImagingFactory(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDecoderFromFilename(string filename, Guid? guidVendorRef, int desiredAccess, DecodeOptions metadataOptions, BitmapDecoder decoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Guid value = default(Guid);
		if (guidVendorRef.HasValue)
		{
			value = guidVendorRef.Value;
		}
		Result result;
		fixed (char* ptr = filename)
		{
			void* nativePointer = _nativePointer;
			Guid* intPtr = ((!guidVendorRef.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(nativePointer, ptr, intPtr, desiredAccess, (int)metadataOptions, &zero);
		}
		decoderOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDecoderFromStream(IStream streamRef, Guid? guidVendorRef, DecodeOptions metadataOptions, BitmapDecoder decoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(streamRef);
		Guid value = default(Guid);
		if (guidVendorRef.HasValue)
		{
			value = guidVendorRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		Guid* intPtr2 = ((!guidVendorRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, (int)metadataOptions, &zero2);
		decoderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateDecoderFromFileHandle(IntPtr hFile, Guid? guidVendorRef, DecodeOptions metadataOptions, BitmapDecoder decoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Guid value = default(Guid);
		if (guidVendorRef.HasValue)
		{
			value = guidVendorRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)hFile;
		Guid* intPtr2 = ((!guidVendorRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, (int)metadataOptions, &zero);
		decoderOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateComponentInfo(Guid clsidComponent, ComponentInfo infoOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &clsidComponent, &zero);
		infoOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDecoder(Guid guidContainerFormat, Guid? guidVendorRef, BitmapDecoder decoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Guid value = default(Guid);
		if (guidVendorRef.HasValue)
		{
			value = guidVendorRef.Value;
		}
		void* nativePointer = _nativePointer;
		Guid* num = &guidContainerFormat;
		Guid* intPtr = ((!guidVendorRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, num, intPtr, &zero);
		decoderOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateEncoder(Guid guidContainerFormat, Guid? guidVendorRef, BitmapEncoder encoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Guid value = default(Guid);
		if (guidVendorRef.HasValue)
		{
			value = guidVendorRef.Value;
		}
		void* nativePointer = _nativePointer;
		Guid* num = &guidContainerFormat;
		Guid* intPtr = ((!guidVendorRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, num, intPtr, &zero);
		encoderOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreatePalette(Palette paletteOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &zero);
		paletteOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateFormatConverter(FormatConverter formatConverterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		formatConverterOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapScaler(BitmapScaler bitmapScalerOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmapScalerOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapClipper(BitmapClipper bitmapClipperOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmapClipperOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFlipRotator(BitmapFlipRotator bitmapFlipRotatorOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmapFlipRotatorOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateStream(WICStream wICStreamOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, &zero);
		wICStreamOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateColorContext(ColorContext wICColorContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, &zero);
		wICColorContextOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateColorTransformer(ColorTransform wICColorTransformOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, &zero);
		wICColorTransformOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmap(int width, int height, Guid ixelFormatRef, BitmapCreateCacheOption option, Bitmap bitmapOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, width, height, &ixelFormatRef, (int)option, &zero);
		bitmapOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromSource(BitmapSource bitmapSourceRef, BitmapCreateCacheOption option, Bitmap bitmapOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(bitmapSourceRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)option, &zero2);
		bitmapOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromSourceRect(BitmapSource bitmapSourceRef, int x, int y, int width, int height, Bitmap bitmapOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(bitmapSourceRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, x, y, width, height, &zero2);
		bitmapOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromMemory(int width, int height, Guid ixelFormatRef, int stride, int bufferSize, IntPtr bufferRef, Bitmap bitmapOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, width, height, &ixelFormatRef, stride, bufferSize, (void*)bufferRef, &zero);
		bitmapOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromHBITMAP(IntPtr hBitmap, IntPtr hPalette, BitmapAlphaChannelOption options, Bitmap bitmapOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)hBitmap, (void*)hPalette, (int)options, &zero);
		bitmapOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromHICON(IntPtr hIcon, Bitmap bitmapOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (void*)hIcon, &zero);
		bitmapOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateComponentEnumerator(int componentTypes, int options, ComObject enumUnknownOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, componentTypes, options, &zero);
		enumUnknownOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateFastMetadataEncoderFromDecoder(BitmapDecoder decoderRef, FastMetadataEncoder fastEncoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapDecoder>(decoderRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		fastEncoderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateFastMetadataEncoderFromFrameDecode(BitmapFrameDecode frameDecoderRef, FastMetadataEncoder fastEncoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapFrameDecode>(frameDecoderRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		fastEncoderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateQueryWriter(Guid guidMetadataFormat, Guid? guidVendorRef, MetadataQueryWriter queryWriterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Guid value = default(Guid);
		if (guidVendorRef.HasValue)
		{
			value = guidVendorRef.Value;
		}
		void* nativePointer = _nativePointer;
		Guid* num = &guidMetadataFormat;
		Guid* intPtr = ((!guidVendorRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(nativePointer, num, intPtr, &zero);
		queryWriterOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateQueryWriterFromReader(MetadataQueryReader queryReaderRef, Guid? guidVendorRef, MetadataQueryWriter queryWriterOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<MetadataQueryReader>(queryReaderRef);
		Guid value = default(Guid);
		if (guidVendorRef.HasValue)
		{
			value = guidVendorRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		Guid* intPtr2 = ((!guidVendorRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		queryWriterOut.NativePointer = zero2;
		result.CheckError();
	}
}

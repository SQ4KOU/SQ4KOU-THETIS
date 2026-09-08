using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("B84E2C09-78C9-4AC4-8BD3-524AE1663A2F")]
public class FastMetadataEncoder : ComObject
{
	public MetadataQueryWriter MetadataQueryWriter
	{
		get
		{
			GetMetadataQueryWriter(out var metadataQueryWriterOut);
			return metadataQueryWriterOut;
		}
	}

	public FastMetadataEncoder(ImagingFactory factory, BitmapDecoder decoder)
		: base(IntPtr.Zero)
	{
		factory.CreateFastMetadataEncoderFromDecoder(decoder, this);
	}

	public FastMetadataEncoder(ImagingFactory factory, BitmapFrameDecode frameDecoder)
		: base(IntPtr.Zero)
	{
		factory.CreateFastMetadataEncoderFromFrameDecode(frameDecoder, this);
	}

	public FastMetadataEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FastMetadataEncoder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FastMetadataEncoder(nativePtr);
		}
		return null;
	}

	public unsafe void Commit()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void GetMetadataQueryWriter(out MetadataQueryWriter metadataQueryWriterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
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
}

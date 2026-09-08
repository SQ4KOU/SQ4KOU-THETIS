using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("E87A44C4-B76E-4c47-8B09-298EB12A2714")]
public class BitmapCodecInfo : ComponentInfo
{
	public Guid[] PixelFormats
	{
		get
		{
			int actualRef = 0;
			GetPixelFormats(0, null, out actualRef);
			if (actualRef == 0)
			{
				return new Guid[0];
			}
			Guid[] array = new Guid[actualRef];
			GetPixelFormats(actualRef, array, out actualRef);
			return array;
		}
	}

	public unsafe string ColorManagementVersion
	{
		get
		{
			int cchActualRef = 0;
			GetColorManagementVersion(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetColorManagementVersion(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string DeviceManufacturer
	{
		get
		{
			int cchActualRef = 0;
			GetDeviceManufacturer(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetDeviceManufacturer(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string DeviceModels
	{
		get
		{
			int cchActualRef = 0;
			GetDeviceModels(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetDeviceModels(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string MimeTypes
	{
		get
		{
			int cchActualRef = 0;
			GetMimeTypes(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetMimeTypes(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string FileExtensions
	{
		get
		{
			int cchActualRef = 0;
			GetFileExtensions(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetFileExtensions(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public Guid ContainerFormat
	{
		get
		{
			GetContainerFormat(out var guidContainerFormatRef);
			return guidContainerFormatRef;
		}
	}

	public RawBool IsAnimationSupported
	{
		get
		{
			IsAnimationSupported_(out var fSupportAnimationRef);
			return fSupportAnimationRef;
		}
	}

	public RawBool IsChromakeySupported
	{
		get
		{
			IsChromakeySupported_(out var fSupportChromakeyRef);
			return fSupportChromakeyRef;
		}
	}

	public RawBool IsLosslessSupported
	{
		get
		{
			IsLosslessSupported_(out var fSupportLosslessRef);
			return fSupportLosslessRef;
		}
	}

	public RawBool IsMultiframeSupported
	{
		get
		{
			IsMultiframeSupported_(out var fSupportMultiframeRef);
			return fSupportMultiframeRef;
		}
	}

	public BitmapCodecInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapCodecInfo(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapCodecInfo(nativePtr);
		}
		return null;
	}

	internal unsafe void GetContainerFormat(out Guid guidContainerFormatRef)
	{
		guidContainerFormatRef = default(Guid);
		Result result;
		fixed (Guid* ptr = &guidContainerFormatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetPixelFormats(int formats, Guid[] guidPixelFormatsRef, out int actualRef)
	{
		Result result;
		fixed (int* ptr = &actualRef)
		{
			void* ptr2 = ptr;
			fixed (Guid* ptr3 = guidPixelFormatsRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, formats, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetColorManagementVersion(int cchColorManagementVersion, IntPtr colorManagementVersion, out int cchActualRef)
	{
		Result result;
		fixed (int* ptr = &cchActualRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, cchColorManagementVersion, (void*)colorManagementVersion, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetDeviceManufacturer(int cchDeviceManufacturer, IntPtr deviceManufacturer, out int cchActualRef)
	{
		Result result;
		fixed (int* ptr = &cchActualRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, cchDeviceManufacturer, (void*)deviceManufacturer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetDeviceModels(int cchDeviceModels, IntPtr deviceModels, out int cchActualRef)
	{
		Result result;
		fixed (int* ptr = &cchActualRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, cchDeviceModels, (void*)deviceModels, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetMimeTypes(int cchMimeTypes, IntPtr mimeTypes, out int cchActualRef)
	{
		Result result;
		fixed (int* ptr = &cchActualRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, cchMimeTypes, (void*)mimeTypes, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetFileExtensions(int cchFileExtensions, IntPtr fileExtensions, out int cchActualRef)
	{
		Result result;
		fixed (int* ptr = &cchActualRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, cchFileExtensions, (void*)fileExtensions, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void IsAnimationSupported_(out RawBool fSupportAnimationRef)
	{
		fSupportAnimationRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fSupportAnimationRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void IsChromakeySupported_(out RawBool fSupportChromakeyRef)
	{
		fSupportChromakeyRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fSupportChromakeyRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void IsLosslessSupported_(out RawBool fSupportLosslessRef)
	{
		fSupportLosslessRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fSupportLosslessRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void IsMultiframeSupported_(out RawBool fSupportMultiframeRef)
	{
		fSupportMultiframeRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fSupportMultiframeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe RawBool MatchesMimeType(string mimeType)
	{
		Result result;
		RawBool result2 = default(RawBool);
		fixed (char* ptr = mimeType)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, ptr, &result2);
		}
		result.CheckError();
		return result2;
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("DAAC296F-7AA5-4dbf-8D15-225C5976F891")]
public class ProgressiveLevelControl : ComObject
{
	public int LevelCount
	{
		get
		{
			GetLevelCount(out var levelsRef);
			return levelsRef;
		}
	}

	public int CurrentLevel
	{
		get
		{
			GetCurrentLevel(out var nLevelRef);
			return nLevelRef;
		}
		set
		{
			SetCurrentLevel(value);
		}
	}

	public ProgressiveLevelControl(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ProgressiveLevelControl(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ProgressiveLevelControl(nativePtr);
		}
		return null;
	}

	internal unsafe void GetLevelCount(out int levelsRef)
	{
		Result result;
		fixed (int* ptr = &levelsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetCurrentLevel(out int nLevelRef)
	{
		Result result;
		fixed (int* ptr = &nLevelRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void SetCurrentLevel(int nLevel)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, nLevel)).CheckError();
	}
}

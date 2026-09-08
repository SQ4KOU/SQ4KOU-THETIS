using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("f292e401-c050-4cde-83d7-04962d3b23c2")]
public class GradientMesh : Resource
{
	public int PatchCount => GetPatchCount();

	public GradientMesh(DeviceContext2 context2, GradientMeshPatch[] atchesRef, int patchesCount)
		: this(IntPtr.Zero)
	{
		context2.CreateGradientMesh(atchesRef, patchesCount, this);
	}

	public GradientMesh(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GradientMesh(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GradientMesh(nativePtr);
		}
		return null;
	}

	internal unsafe int GetPatchCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetPatches(int startIndex, GradientMeshPatch[] atchesRef, int patchesCount)
	{
		Result result;
		fixed (GradientMeshPatch* ptr = atchesRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, startIndex, ptr2, patchesCount);
		}
		result.CheckError();
	}
}

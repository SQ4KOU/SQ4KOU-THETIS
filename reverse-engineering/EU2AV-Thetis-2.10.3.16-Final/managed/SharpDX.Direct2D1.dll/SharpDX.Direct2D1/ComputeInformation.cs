using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("5598b14b-9fd7-48b7-9bdb-8f0964eb38bc")]
public class ComputeInformation : RenderInformation
{
	public Guid ComputeShader
	{
		set
		{
			SetComputeShader(value);
		}
	}

	public void SetConstantBuffer(DataStream dataStream)
	{
		SetComputeShaderConstantBuffer(dataStream.DataPointer, (int)dataStream.Length);
	}

	public unsafe void SetConstantBuffer<T>(T value) where T : struct
	{
		fixed (T* ptr = &System.Runtime.CompilerServices.Unsafe.AsRef<T>(&value))
		{
			SetComputeShaderConstantBuffer((IntPtr)ptr, Utilities.SizeOf<T>());
		}
	}

	public unsafe void SetConstantBuffer<T>(ref T value) where T : struct
	{
		fixed (T* ptr = &value)
		{
			SetComputeShaderConstantBuffer((IntPtr)ptr, Utilities.SizeOf<T>());
		}
	}

	public ComputeInformation(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ComputeInformation(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ComputeInformation(nativePtr);
		}
		return null;
	}

	internal unsafe void SetComputeShaderConstantBuffer(IntPtr buffer, int bufferCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)buffer, bufferCount)).CheckError();
	}

	internal unsafe void SetComputeShader(Guid shaderId)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &shaderId)).CheckError();
	}

	public unsafe void SetResourceTexture(int textureIndex, ResourceTexture resourceTexture)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ResourceTexture>(resourceTexture);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, textureIndex, (void*)zero)).CheckError();
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("693ce632-7f2f-45de-93fe-18d88b37aa21")]
public class DrawInformation : RenderInformation
{
	public void SetVertexConstantBuffer(DataStream dataStream)
	{
		SetVertexShaderConstantBuffer(dataStream.DataPointer, (int)dataStream.Length);
	}

	public unsafe void SetVertexConstantBuffer<T>(T value) where T : struct
	{
		fixed (T* ptr = &System.Runtime.CompilerServices.Unsafe.AsRef<T>(&value))
		{
			SetVertexShaderConstantBuffer((IntPtr)ptr, Utilities.SizeOf<T>());
		}
	}

	public unsafe void SetVertexConstantBuffer<T>(ref T value) where T : struct
	{
		fixed (T* ptr = &value)
		{
			SetVertexShaderConstantBuffer((IntPtr)ptr, Utilities.SizeOf<T>());
		}
	}

	public void SetPixelConstantBuffer(DataStream dataStream)
	{
		SetPixelShaderConstantBuffer(dataStream.DataPointer, (int)dataStream.Length);
	}

	public unsafe void SetPixelConstantBuffer<T>(T value) where T : struct
	{
		fixed (T* ptr = &System.Runtime.CompilerServices.Unsafe.AsRef<T>(&value))
		{
			SetPixelShaderConstantBuffer((IntPtr)ptr, Utilities.SizeOf<T>());
		}
	}

	public unsafe void SetPixelConstantBuffer<T>(ref T value) where T : struct
	{
		fixed (T* ptr = &value)
		{
			SetPixelShaderConstantBuffer((IntPtr)ptr, Utilities.SizeOf<T>());
		}
	}

	public DrawInformation(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DrawInformation(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DrawInformation(nativePtr);
		}
		return null;
	}

	internal unsafe void SetPixelShaderConstantBuffer(IntPtr buffer, int bufferCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)buffer, bufferCount)).CheckError();
	}

	public unsafe void SetResourceTexture(int textureIndex, ResourceTexture resourceTexture)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ResourceTexture>(resourceTexture);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, textureIndex, (void*)zero)).CheckError();
	}

	internal unsafe void SetVertexShaderConstantBuffer(IntPtr buffer, int bufferCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)buffer, bufferCount)).CheckError();
	}

	public unsafe void SetPixelShader(Guid shaderId, PixelOptions pixelOptions)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &shaderId, (int)pixelOptions)).CheckError();
	}

	public unsafe void SetVertexProcessing(VertexBuffer vertexBuffer, VertexOptions vertexOptions, BlendDescription? blendDescription = null, VertexRange? vertexRange = null, Guid? vertexShader = null)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VertexBuffer>(vertexBuffer);
		BlendDescription value = default(BlendDescription);
		if (blendDescription.HasValue)
		{
			value = blendDescription.Value;
		}
		VertexRange value2 = default(VertexRange);
		if (vertexRange.HasValue)
		{
			value2 = vertexRange.Value;
		}
		Guid value3 = default(Guid);
		if (vertexShader.HasValue)
		{
			value3 = vertexShader.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		BlendDescription* intPtr2 = ((!blendDescription.HasValue) ? null : (&value));
		VertexRange* intPtr3 = ((!vertexRange.HasValue) ? null : (&value2));
		Guid* intPtr4 = ((!vertexShader.HasValue) ? null : (&value3));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, intPtr, (int)vertexOptions, intPtr2, intPtr3, intPtr4)).CheckError();
	}
}

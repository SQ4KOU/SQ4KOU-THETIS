using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("9b8b1336-00a5-4668-92b7-ced5d8bf9b7b")]
public class VertexBuffer : ComObject
{
	public unsafe VertexBuffer(EffectContext context, Guid resourceId, VertexBufferProperties vertexBufferProperties, CustomVertexBufferProperties customVertexBufferProperties)
		: base(IntPtr.Zero)
	{
		CustomVertexBufferProperties.__Native @ref = default(CustomVertexBufferProperties.__Native);
		customVertexBufferProperties.__MarshalTo(ref @ref);
		InputElement.__Native[] array = new InputElement.__Native[customVertexBufferProperties.InputElements.Length];
		try
		{
			for (int i = 0; i < customVertexBufferProperties.InputElements.Length; i++)
			{
				customVertexBufferProperties.InputElements[i].__MarshalTo(ref array[i]);
			}
			fixed (InputElement.__Native* ptr = array)
			{
				void* ptr2 = ptr;
				@ref.InputElementsPointer = (IntPtr)ptr2;
				@ref.ElementCount = customVertexBufferProperties.InputElements.Length;
				fixed (byte* inputSignature = customVertexBufferProperties.InputSignature)
				{
					void* ptr3 = inputSignature;
					@ref.ShaderBufferSize = customVertexBufferProperties.InputSignature.Length;
					@ref.ShaderBufferWithInputSignature = (IntPtr)ptr3;
					context.CreateVertexBuffer(vertexBufferProperties, resourceId, new IntPtr(&@ref), this);
				}
			}
		}
		finally
		{
			for (int j = 0; j < array.Length; j++)
			{
				customVertexBufferProperties.InputElements[j].__MarshalFree(ref array[j]);
			}
		}
	}

	public VertexBuffer(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VertexBuffer(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VertexBuffer(nativePtr);
		}
		return null;
	}

	public unsafe void Map(byte[] data, int bufferSize)
	{
		Result result;
		fixed (byte* ptr = data)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2, bufferSize);
		}
		result.CheckError();
	}

	public unsafe void Unmap()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}

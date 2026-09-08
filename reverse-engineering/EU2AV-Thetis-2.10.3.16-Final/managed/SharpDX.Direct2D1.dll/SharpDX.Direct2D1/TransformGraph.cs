using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("13d29038-c3e6-4034-9081-13b53a417992")]
public class TransformGraph : ComObject
{
	public int InputCount => GetInputCount();

	public TransformGraph(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TransformGraph(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TransformGraph(nativePtr);
		}
		return null;
	}

	internal unsafe int GetInputCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void SetSingleTransformNode(TransformNode node)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TransformNode>(node);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void AddNode(TransformNode node)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TransformNode>(node);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void RemoveNode(TransformNode node)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TransformNode>(node);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void SetOutputNode(TransformNode node)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TransformNode>(node);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void ConnectNode(TransformNode fromNode, TransformNode toNode, int toNodeInputIndex)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TransformNode>(fromNode);
		zero2 = CppObject.ToCallbackPtr<TransformNode>(toNode);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, toNodeInputIndex)).CheckError();
	}

	public unsafe void ConnectToEffectInput(int toEffectInputIndex, TransformNode node, int toNodeInputIndex)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TransformNode>(node);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, toEffectInputIndex, (void*)zero, toNodeInputIndex)).CheckError();
	}

	public unsafe void Clear()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void SetPassthroughGraph(int effectInputIndex)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, effectInputIndex)).CheckError();
	}
}

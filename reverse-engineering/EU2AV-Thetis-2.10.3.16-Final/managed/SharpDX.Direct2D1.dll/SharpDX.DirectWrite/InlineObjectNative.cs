using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("8339FDE3-106F-47ab-8373-1C6295EB10B3")]
public class InlineObjectNative : ComObject, InlineObject, IUnknown, ICallbackable, IDisposable
{
	public InlineObjectMetrics Metrics
	{
		get
		{
			GetMetrics_(out var metrics);
			return metrics;
		}
	}

	public OverhangMetrics OverhangMetrics
	{
		get
		{
			GetOverhangMetrics_(out var overhangs);
			return overhangs;
		}
	}

	public InlineObjectMetrics Metrics_
	{
		get
		{
			GetMetrics_(out var metrics);
			return metrics;
		}
	}

	public OverhangMetrics OverhangMetrics_
	{
		get
		{
			GetOverhangMetrics_(out var overhangs);
			return overhangs;
		}
	}

	public void Draw(object clientDrawingContext, TextRenderer renderer, float originX, float originY, bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect)
	{
		GCHandle value = GCHandle.Alloc(clientDrawingContext);
		IntPtr iUnknownForObject = Utilities.GetIUnknownForObject(clientDrawingEffect);
		try
		{
			Draw_(GCHandle.ToIntPtr(value), renderer, originX, originY, isSideways, isRightToLeft, iUnknownForObject);
		}
		finally
		{
			if (value.IsAllocated)
			{
				value.Free();
			}
			if (iUnknownForObject != IntPtr.Zero)
			{
				Marshal.Release(iUnknownForObject);
			}
		}
	}

	public void GetBreakConditions(out BreakCondition breakConditionBefore, out BreakCondition breakConditionAfter)
	{
		GetBreakConditions_(out breakConditionBefore, out breakConditionAfter);
	}

	public InlineObjectNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator InlineObjectNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new InlineObjectNative(nativePtr);
		}
		return null;
	}

	internal unsafe void Draw_(IntPtr clientDrawingContext, TextRenderer renderer, float originX, float originY, RawBool isSideways, RawBool isRightToLeft, IntPtr clientDrawingEffect)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextRenderer>(renderer);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, float, RawBool, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)clientDrawingContext, (void*)zero, originX, originY, isSideways, isRightToLeft, (void*)clientDrawingEffect)).CheckError();
	}

	internal unsafe void GetMetrics_(out InlineObjectMetrics metrics)
	{
		metrics = default(InlineObjectMetrics);
		Result result;
		fixed (InlineObjectMetrics* ptr = &metrics)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetOverhangMetrics_(out OverhangMetrics overhangs)
	{
		overhangs = default(OverhangMetrics);
		Result result;
		fixed (OverhangMetrics* ptr = &overhangs)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetBreakConditions_(out BreakCondition breakConditionBefore, out BreakCondition breakConditionAfter)
	{
		Result result;
		fixed (BreakCondition* ptr = &breakConditionAfter)
		{
			void* ptr2 = ptr;
			fixed (BreakCondition* ptr3 = &breakConditionBefore)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}
}

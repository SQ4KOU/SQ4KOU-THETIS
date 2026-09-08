using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

internal class TransformShadow : TransformNodeShadow
{
	public class TransformVtbl : TransformNodeVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int MapOutputRectToInputRectsDelegate(IntPtr thisPtr, IntPtr outputRect, IntPtr inputRects, int inputRectsCount);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int MapInputRectsToOutputRectDelegate(IntPtr thisPtr, IntPtr inputRects, IntPtr inputOpaqueSubRects, int inputRectsCount, IntPtr outputRect, IntPtr outputOpaqueSubRect);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int MapInvalidRectDelegate(IntPtr thisPtr, int inputIndex, IntPtr invalidInputRect, IntPtr invalidOutputRect);

		public TransformVtbl(int methods)
			: base(3 + methods)
		{
			AddMethod(new MapOutputRectToInputRectsDelegate(MapOutputRectToInputRectsImpl));
			AddMethod(new MapInputRectsToOutputRectDelegate(MapInputRectsToOutputRectImpl));
			AddMethod(new MapInvalidRectDelegate(MapInvalidRectImpl));
		}

		private unsafe static int MapOutputRectToInputRectsImpl(IntPtr thisPtr, IntPtr outputRect, IntPtr inputRects, int inputRectsCount)
		{
			try
			{
				Transform obj = (Transform)CppObjectShadow.ToShadow<TransformShadow>(thisPtr).Callback;
				RawRectangle[] array = new RawRectangle[inputRectsCount];
				Utilities.Read(inputRects, array, 0, inputRectsCount);
				obj.MapOutputRectangleToInputRectangles(*(RawRectangle*)(void*)outputRect, array);
				Utilities.Write(inputRects, array, 0, inputRectsCount);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int MapInputRectsToOutputRectImpl(IntPtr thisPtr, IntPtr inputRects, IntPtr inputOpaqueSubRects, int inputRectsCount, IntPtr outputRect, IntPtr outputOpaqueSubRect)
		{
			try
			{
				Transform transform = (Transform)CppObjectShadow.ToShadow<TransformShadow>(thisPtr).Callback;
				RawRectangle[] array = new RawRectangle[inputRectsCount];
				Utilities.Read(inputRects, array, 0, inputRectsCount);
				RawRectangle[] array2 = new RawRectangle[inputRectsCount];
				Utilities.Read(inputOpaqueSubRects, array2, 0, inputRectsCount);
				*(RawRectangle*)(void*)outputRect = transform.MapInputRectanglesToOutputRectangle(array, array2, out *(RawRectangle*)(void*)outputOpaqueSubRect);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int MapInvalidRectImpl(IntPtr thisPtr, int inputIndex, IntPtr invalidInputRect, IntPtr invalidOutputRect)
		{
			try
			{
				Transform transform = (Transform)CppObjectShadow.ToShadow<TransformShadow>(thisPtr).Callback;
				*(RawRectangle*)(void*)invalidOutputRect = transform.MapInvalidRect(inputIndex, *(RawRectangle*)(void*)invalidInputRect);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly TransformVtbl Vtbl = new TransformVtbl(0);

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(Transform callback)
	{
		return CppObject.ToCallbackPtr<Transform>(callback);
	}
}

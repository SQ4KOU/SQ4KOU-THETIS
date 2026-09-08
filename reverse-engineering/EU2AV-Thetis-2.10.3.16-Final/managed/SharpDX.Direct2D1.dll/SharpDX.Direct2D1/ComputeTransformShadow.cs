using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

internal class ComputeTransformShadow : TransformShadow
{
	public class ComputeTransformVtbl : TransformVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetComputeInformationDelegate(IntPtr thisPtr, IntPtr computeInfo);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CalculateThreadgroupsDelegate(IntPtr thisPtr, IntPtr rect, out int dimX, out int dimY, out int dimZ);

		public ComputeTransformVtbl(int methods)
			: base(2 + methods)
		{
			AddMethod(new SetComputeInformationDelegate(SetComputeInformationImpl));
			AddMethod(new CalculateThreadgroupsDelegate(CalculateThreadgroupsImpl));
		}

		private static int SetComputeInformationImpl(IntPtr thisPtr, IntPtr computeInfo)
		{
			try
			{
				((ComputeTransform)CppObjectShadow.ToShadow<ComputeTransformShadow>(thisPtr).Callback).SetComputeInformation(new ComputeInformation(computeInfo));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int CalculateThreadgroupsImpl(IntPtr thisPtr, IntPtr rect, out int dimX, out int dimY, out int dimZ)
		{
			dimX = (dimY = (dimZ = 0));
			try
			{
				RawInt3 rawInt = ((ComputeTransform)CppObjectShadow.ToShadow<ComputeTransformShadow>(thisPtr).Callback).CalculateThreadgroups(*(RawRectangle*)(void*)rect);
				dimX = rawInt.X;
				dimY = rawInt.Y;
				dimZ = rawInt.Z;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly ComputeTransformVtbl Vtbl = new ComputeTransformVtbl(0);

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(ComputeTransform callback)
	{
		return CppObject.ToCallbackPtr<ComputeTransform>(callback);
	}
}

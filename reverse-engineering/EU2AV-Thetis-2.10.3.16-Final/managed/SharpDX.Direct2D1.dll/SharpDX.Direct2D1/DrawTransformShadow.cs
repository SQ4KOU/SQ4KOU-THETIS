using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

internal class DrawTransformShadow : TransformShadow
{
	public class DrawTransformVtbl : TransformVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetDrawInfoDelegate(IntPtr thisPtr, IntPtr drawInfo);

		public DrawTransformVtbl(int methods)
			: base(1 + methods)
		{
			AddMethod(new SetDrawInfoDelegate(SetDrawInfoImpl));
		}

		private static int SetDrawInfoImpl(IntPtr thisPtr, IntPtr drawInfo)
		{
			try
			{
				((DrawTransform)CppObjectShadow.ToShadow<DrawTransformShadow>(thisPtr).Callback).SetDrawInformation(new DrawInformation(drawInfo));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly DrawTransformVtbl Vtbl = new DrawTransformVtbl(0);

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(DrawTransform callback)
	{
		return CppObject.ToCallbackPtr<DrawTransform>(callback);
	}
}

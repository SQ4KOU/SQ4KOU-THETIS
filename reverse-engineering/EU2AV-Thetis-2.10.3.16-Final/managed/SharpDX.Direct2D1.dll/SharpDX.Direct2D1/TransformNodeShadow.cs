using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

internal class TransformNodeShadow : ComObjectShadow
{
	public class TransformNodeVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetInputCountDelegate(IntPtr thisPtr);

		public TransformNodeVtbl(int methods)
			: base(1 + methods)
		{
			AddMethod(new GetInputCountDelegate(GetInputCountImpl));
		}

		private static int GetInputCountImpl(IntPtr thisPtr)
		{
			return ((TransformNode)CppObjectShadow.ToShadow<TransformNodeShadow>(thisPtr).Callback).InputCount;
		}
	}

	private static readonly TransformNodeVtbl Vtbl = new TransformNodeVtbl(0);

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(TransformNode callback)
	{
		return CppObject.ToCallbackPtr<TransformNode>(callback);
	}
}

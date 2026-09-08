using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

internal class SourceTransformShadow : TransformShadow
{
	public class SourceTransformVtbl : TransformVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetRenderInformationDelegate(IntPtr thisPtr, IntPtr renderInfo);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawDelegate(IntPtr thisPtr, IntPtr target, IntPtr drawRect, RawPoint targetOrigin);

		public SourceTransformVtbl(int methods)
			: base(2 + methods)
		{
			AddMethod(new SetRenderInformationDelegate(SetRenderInformationImpl));
			AddMethod(new DrawDelegate(DrawImpl));
		}

		private static int SetRenderInformationImpl(IntPtr thisPtr, IntPtr renderInfo)
		{
			try
			{
				((SourceTransform)CppObjectShadow.ToShadow<SourceTransformShadow>(thisPtr).Callback).SetRenderInformation(new RenderInformation(renderInfo));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int DrawImpl(IntPtr thisPtr, IntPtr target, IntPtr drawRect, RawPoint targetOrigin)
		{
			try
			{
				((SourceTransform)CppObjectShadow.ToShadow<SourceTransformShadow>(thisPtr).Callback).Draw(new Bitmap1(target), *(RawRectangle*)(void*)drawRect, targetOrigin);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly SourceTransformVtbl Vtbl = new SourceTransformVtbl(0);

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(SourceTransform callback)
	{
		return CppObject.ToCallbackPtr<SourceTransform>(callback);
	}
}

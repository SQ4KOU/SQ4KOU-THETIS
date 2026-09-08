using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

internal class CustomEffectShadow : ComObjectShadow
{
	public class CustomEffectVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int InitializeDelegate(IntPtr thisPtr, IntPtr effectContext, IntPtr transformationGraph);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int PrepareForRenderDelegate(IntPtr thisPtr, ChangeType changeType);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetGraphDelegate(IntPtr thisPtr, IntPtr transformGraph);

		public CustomEffectVtbl()
			: base(3)
		{
			AddMethod(new InitializeDelegate(InitializeImpl));
			AddMethod(new PrepareForRenderDelegate(PrepareForRenderImpl));
			AddMethod(new SetGraphDelegate(SetGraphImpl));
		}

		private static int InitializeImpl(IntPtr thisPtr, IntPtr effectContext, IntPtr transformationGraph)
		{
			try
			{
				((CustomEffect)CppObjectShadow.ToShadow<CustomEffectShadow>(thisPtr).Callback).Initialize(new EffectContext(effectContext), new TransformGraph(transformationGraph));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int PrepareForRenderImpl(IntPtr thisPtr, ChangeType changeType)
		{
			try
			{
				((CustomEffect)CppObjectShadow.ToShadow<CustomEffectShadow>(thisPtr).Callback).PrepareForRender(changeType);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetGraphImpl(IntPtr thisPtr, IntPtr transformGraph)
		{
			try
			{
				((CustomEffect)CppObjectShadow.ToShadow<CustomEffectShadow>(thisPtr).Callback).SetGraph(new TransformGraph(transformGraph));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly CustomEffectVtbl Vtbl = new CustomEffectVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(CustomEffect callback)
	{
		return CppObject.ToCallbackPtr<CustomEffect>(callback);
	}
}

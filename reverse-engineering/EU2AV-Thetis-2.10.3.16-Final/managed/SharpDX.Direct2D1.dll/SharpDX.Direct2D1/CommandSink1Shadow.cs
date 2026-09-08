using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

internal class CommandSink1Shadow : ComObjectShadow
{
	public class CommandSink1Vtbl : CommandSinkShadow.CommandSinkVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetPrimitiveBlend1Delegate(IntPtr thisPtr, PrimitiveBlend primitiveBlend);

		public CommandSink1Vtbl()
			: base(1)
		{
			AddMethod(new SetPrimitiveBlend1Delegate(SetPrimitiveBlend1Impl));
		}

		private static int SetPrimitiveBlend1Impl(IntPtr thisPtr, PrimitiveBlend primitiveBlend)
		{
			try
			{
				((CommandSink1)CppObjectShadow.ToShadow<CommandSink1Shadow>(thisPtr).Callback).PrimitiveBlend1 = primitiveBlend;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly CommandSink1Vtbl Vtbl = new CommandSink1Vtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(CommandSink1 callback)
	{
		return CppObject.ToCallbackPtr<CommandSink1>(callback);
	}
}

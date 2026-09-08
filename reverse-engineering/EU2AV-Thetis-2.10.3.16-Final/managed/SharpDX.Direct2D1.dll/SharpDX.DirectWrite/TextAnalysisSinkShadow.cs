using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class TextAnalysisSinkShadow : ComObjectShadow
{
	protected class TextAnalysisSinkVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private unsafe delegate int SetScriptAnalysisDelegate(IntPtr thisPtr, int textPosition, int textLength, ScriptAnalysis* scriptAnalysis);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetLineBreakpointsDelegate(IntPtr thisPtr, int textPosition, int textLength, IntPtr pLineBreakpoints);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetBidiLevelDelegate(IntPtr thisPtr, int textPosition, int textLength, byte explicitLevel, byte resolvedLevel);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetNumberSubstitutionDelegate(IntPtr thisPtr, int textPosition, int textLength, IntPtr numberSubstitution);

		public unsafe TextAnalysisSinkVtbl(int methodCount = 0)
			: base(4 + methodCount)
		{
			AddMethod(new SetScriptAnalysisDelegate(SetScriptAnalysisImpl));
			AddMethod(new SetLineBreakpointsDelegate(SetLineBreakpointsImpl));
			AddMethod(new SetBidiLevelDelegate(SetBidiLevelImpl));
			AddMethod(new SetNumberSubstitutionDelegate(SetNumberSubstitutionImpl));
		}

		private unsafe static int SetScriptAnalysisImpl(IntPtr thisPtr, int textPosition, int textLength, ScriptAnalysis* scriptAnalysis)
		{
			try
			{
				((TextAnalysisSink)CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr).Callback).SetScriptAnalysis(textPosition, textLength, *scriptAnalysis);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetLineBreakpointsImpl(IntPtr thisPtr, int textPosition, int textLength, IntPtr pLineBreakpoints)
		{
			try
			{
				TextAnalysisSink obj = (TextAnalysisSink)CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr).Callback;
				LineBreakpoint[] array = new LineBreakpoint[textLength];
				Utilities.Read(pLineBreakpoints, array, 0, textLength);
				obj.SetLineBreakpoints(textPosition, textLength, array);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetBidiLevelImpl(IntPtr thisPtr, int textPosition, int textLength, byte explicitLevel, byte resolvedLevel)
		{
			try
			{
				((TextAnalysisSink)CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr).Callback).SetBidiLevel(textPosition, textLength, explicitLevel, resolvedLevel);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetNumberSubstitutionImpl(IntPtr thisPtr, int textPosition, int textLength, IntPtr numberSubstitution)
		{
			try
			{
				((TextAnalysisSink)CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr).Callback).SetNumberSubstitution(textPosition, textLength, new NumberSubstitution(numberSubstitution));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly TextAnalysisSinkVtbl Vtbl = new TextAnalysisSinkVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(TextAnalysisSink callback)
	{
		return CppObject.ToCallbackPtr<TextAnalysisSink>(callback);
	}
}

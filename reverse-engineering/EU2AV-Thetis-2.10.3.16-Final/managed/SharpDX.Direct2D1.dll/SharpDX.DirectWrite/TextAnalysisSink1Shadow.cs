using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

internal class TextAnalysisSink1Shadow : TextAnalysisSinkShadow
{
	protected class TextAnalysisSink1Vtbl : TextAnalysisSinkVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetGlyphOrientationDelegate(IntPtr thisPtr, int textPosition, int textLength, GlyphOrientationAngle glyphOrientationAngle, byte adjustedBidiLevel, RawBool isSideways, RawBool isRightToLeft);

		public TextAnalysisSink1Vtbl()
			: base(1)
		{
			AddMethod(new SetGlyphOrientationDelegate(SetGlyphOrientationImpl));
		}

		private static int SetGlyphOrientationImpl(IntPtr thisPtr, int textPosition, int textLength, GlyphOrientationAngle glyphOrientationAngle, byte adjustedBidiLevel, RawBool isSideways, RawBool isRightToLeft)
		{
			try
			{
				((TextAnalysisSink1)CppObjectShadow.ToShadow<TextAnalysisSink1Shadow>(thisPtr).Callback).SetGlyphOrientation(textPosition, textLength, glyphOrientationAngle, adjustedBidiLevel, isSideways, isRightToLeft);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly TextAnalysisSink1Vtbl Vtbl = new TextAnalysisSink1Vtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(TextAnalysisSink1 callback)
	{
		return CppObject.ToCallbackPtr<TextAnalysisSink1>(callback);
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class TextAnalysisSource1Shadow : TextAnalysisSourceShadow
{
	private class TextAnalysisSource1Vtbl : TextAnalysisSourceVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetVerticalGlyphOrientationDelegate(IntPtr thisPtr, int textPosition, out int textLength, out VerticalGlyphOrientation glyphOrientation, out byte bidiLevel);

		public TextAnalysisSource1Vtbl()
			: base(1)
		{
			AddMethod(new GetVerticalGlyphOrientationDelegate(GetVerticalGlyphOrientationImpl));
		}

		private static int GetVerticalGlyphOrientationImpl(IntPtr thisPtr, int textPosition, out int textLength, out VerticalGlyphOrientation glyphOrientation, out byte bidiLevel)
		{
			textLength = 0;
			glyphOrientation = VerticalGlyphOrientation.Default;
			bidiLevel = 0;
			try
			{
				((TextAnalysisSource1)CppObjectShadow.ToShadow<TextAnalysisSource1Shadow>(thisPtr).Callback).GetVerticalGlyphOrientation(textPosition, out textLength, out glyphOrientation, out bidiLevel);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly TextAnalysisSource1Vtbl Vtbl = new TextAnalysisSource1Vtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(TextAnalysisSource1 callback)
	{
		return CppObject.ToCallbackPtr<TextAnalysisSource1>(callback);
	}
}

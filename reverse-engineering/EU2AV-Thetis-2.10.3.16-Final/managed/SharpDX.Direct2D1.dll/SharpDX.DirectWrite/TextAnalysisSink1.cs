using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("B0D941A0-85E7-4D8B-9FD3-5CED9934482A")]
[Shadow(typeof(TextAnalysisSink1Shadow))]
public interface TextAnalysisSink1 : TextAnalysisSink, IUnknown, ICallbackable, IDisposable
{
	void SetGlyphOrientation(int textPosition, int textLength, GlyphOrientationAngle glyphOrientationAngle, byte adjustedBidiLevel, RawBool isSideways, RawBool isRightToLeft);
}

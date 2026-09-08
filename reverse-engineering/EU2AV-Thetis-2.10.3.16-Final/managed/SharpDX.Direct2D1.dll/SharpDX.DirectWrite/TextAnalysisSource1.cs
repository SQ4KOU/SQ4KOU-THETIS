using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(TextAnalysisSource1Shadow))]
[Guid("639CFAD8-0FB4-4B21-A58A-067920120009")]
public interface TextAnalysisSource1 : TextAnalysisSource, IUnknown, ICallbackable, IDisposable
{
	void GetVerticalGlyphOrientation(int textPosition, out int textLength, out VerticalGlyphOrientation glyphOrientation, out byte bidiLevel);
}

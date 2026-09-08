using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(TextAnalysisSinkShadow))]
[Guid("5810cd44-0ca0-4701-b3fa-bec5182ae4f6")]
public interface TextAnalysisSink : IUnknown, ICallbackable, IDisposable
{
	void SetScriptAnalysis(int textPosition, int textLength, ScriptAnalysis scriptAnalysis);

	void SetLineBreakpoints(int textPosition, int textLength, LineBreakpoint[] lineBreakpoints);

	void SetBidiLevel(int textPosition, int textLength, byte explicitLevel, byte resolvedLevel);

	void SetNumberSubstitution(int textPosition, int textLength, NumberSubstitution numberSubstitution);
}

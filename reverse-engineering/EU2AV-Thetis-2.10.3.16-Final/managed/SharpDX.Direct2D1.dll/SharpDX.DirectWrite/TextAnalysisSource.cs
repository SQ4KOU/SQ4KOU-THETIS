using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(TextAnalysisSourceShadow))]
[Guid("688e1a58-5094-47c8-adc8-fbcea60ae92b")]
public interface TextAnalysisSource : IUnknown, ICallbackable, IDisposable
{
	ReadingDirection ReadingDirection { get; }

	string GetTextAtPosition(int textPosition);

	string GetTextBeforePosition(int textPosition);

	string GetLocaleName(int textPosition, out int textLength);

	NumberSubstitution GetNumberSubstitution(int textPosition, out int textLength);
}

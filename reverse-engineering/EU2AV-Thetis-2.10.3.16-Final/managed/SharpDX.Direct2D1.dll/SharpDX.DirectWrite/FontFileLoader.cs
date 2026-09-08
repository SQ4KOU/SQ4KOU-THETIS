using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(FontFileLoaderShadow))]
[Guid("727cad4e-d6af-4c9e-8a08-d695b11caa49")]
public interface FontFileLoader : IUnknown, ICallbackable, IDisposable
{
	FontFileStream CreateStreamFromKey(DataPointer fontFileReferenceKey);
}

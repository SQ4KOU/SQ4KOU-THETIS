using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("D3E0E934-22A0-427E-AAE4-7D9574B59DB1")]
public interface TextRenderer1 : TextRenderer, PixelSnapping, IUnknown, ICallbackable, IDisposable
{
}

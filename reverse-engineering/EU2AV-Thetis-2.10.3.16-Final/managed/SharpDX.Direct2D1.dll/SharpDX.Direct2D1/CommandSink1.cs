using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("9eb767fd-4269-4467-b8c2-eb30cb305743")]
public interface CommandSink1 : CommandSink, IUnknown, ICallbackable, IDisposable
{
	PrimitiveBlend PrimitiveBlend1 { set; }
}

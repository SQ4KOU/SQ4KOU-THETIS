using System;
using System.Threading;

namespace SkiaSharp;

public abstract class SKAbstractManagedWStream : SKWStream
{
	private static readonly SKManagedWStreamDelegates delegates;

	internal int fromNative;

	static SKAbstractManagedWStream()
	{
		delegates = new SKManagedWStreamDelegates
		{
			fWrite = DelegateProxies.SKManagedWStreamWriteProxy,
			fFlush = DelegateProxies.SKManagedWStreamFlushProxy,
			fBytesWritten = DelegateProxies.SKManagedWStreamBytesWrittenProxy,
			fDestroy = DelegateProxies.SKManagedWStreamDestroyProxy
		};
		SkiaApi.sk_managedwstream_set_procs(delegates);
	}

	protected SKAbstractManagedWStream()
		: this(owns: true)
	{
	}

	protected unsafe SKAbstractManagedWStream(bool owns)
		: base(IntPtr.Zero, owns)
	{
		IntPtr intPtr = DelegateProxies.CreateUserData(this, makeWeak: true);
		Handle = SkiaApi.sk_managedwstream_new((void*)intPtr);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		if (Interlocked.CompareExchange(ref fromNative, 0, 0) == 0)
		{
			SkiaApi.sk_managedwstream_destroy(Handle);
		}
	}

	protected internal abstract bool OnWrite(IntPtr buffer, IntPtr size);

	protected internal abstract void OnFlush();

	protected internal abstract IntPtr OnBytesWritten();
}

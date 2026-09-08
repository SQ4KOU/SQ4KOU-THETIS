using System;

namespace SkiaSharp;

public class SKTraceMemoryDump : SKObject, ISKSkipObjectRegistration
{
	private static readonly SKManagedTraceMemoryDumpDelegates delegates;

	private readonly IntPtr userData;

	static SKTraceMemoryDump()
	{
		delegates = new SKManagedTraceMemoryDumpDelegates
		{
			fDumpNumericValue = DelegateProxies.SKManagedTraceMemoryDumpDumpNumericValueProxy,
			fDumpStringValue = DelegateProxies.SKManagedTraceMemoryDumpDumpStringValueProxy
		};
		SkiaApi.sk_managedtracememorydump_set_procs(delegates);
	}

	protected unsafe SKTraceMemoryDump(bool detailedDump, bool dumpWrappedObjects)
		: base(IntPtr.Zero, owns: true)
	{
		userData = DelegateProxies.CreateUserData(this, makeWeak: true);
		Handle = SkiaApi.sk_managedtracememorydump_new(detailedDump, dumpWrappedObjects, (void*)userData);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKTraceMemoryDump instance.");
		}
	}

	protected override void DisposeNative()
	{
		DelegateProxies.GetUserData<SKTraceMemoryDump>(userData, out var gch);
		SkiaApi.sk_managedtracememorydump_delete(Handle);
		gch.Free();
	}

	protected internal virtual void OnDumpNumericValue(string dumpName, string valueName, string units, ulong value)
	{
	}

	protected internal virtual void OnDumpStringValue(string dumpName, string valueName, string value)
	{
	}
}

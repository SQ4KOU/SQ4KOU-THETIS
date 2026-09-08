using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp;

internal static class DelegateProxies
{
	public unsafe static readonly GRGlGetProcProxyDelegate GRGlGetProcProxy = GRGlGetProcProxyImplementation;

	public unsafe static readonly GRVkGetProcProxyDelegate GRVkGetProcProxy = GRVkGetProcProxyImplementation;

	public unsafe static readonly SKBitmapReleaseProxyDelegate SKBitmapReleaseProxy = SKBitmapReleaseProxyImplementation;

	public unsafe static readonly SKDataReleaseProxyDelegate SKDataReleaseProxy = SKDataReleaseProxyImplementation;

	public unsafe static readonly SKGlyphPathProxyDelegate SKGlyphPathProxy = SKGlyphPathProxyImplementation;

	public unsafe static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseProxy = SKImageRasterReleaseProxyImplementation;

	public unsafe static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseProxyForCoTaskMem = SKImageRasterReleaseProxyImplementationForCoTaskMem;

	public unsafe static readonly SKImageTextureReleaseProxyDelegate SKImageTextureReleaseProxy = SKImageTextureReleaseProxyImplementation;

	public unsafe static readonly SKManagedDrawableApproximateBytesUsedProxyDelegate SKManagedDrawableApproximateBytesUsedProxy = SKManagedDrawableApproximateBytesUsedProxyImplementation;

	public unsafe static readonly SKManagedDrawableDestroyProxyDelegate SKManagedDrawableDestroyProxy = SKManagedDrawableDestroyProxyImplementation;

	public unsafe static readonly SKManagedDrawableDrawProxyDelegate SKManagedDrawableDrawProxy = SKManagedDrawableDrawProxyImplementation;

	public unsafe static readonly SKManagedDrawableGetBoundsProxyDelegate SKManagedDrawableGetBoundsProxy = SKManagedDrawableGetBoundsProxyImplementation;

	public unsafe static readonly SKManagedDrawableMakePictureSnapshotProxyDelegate SKManagedDrawableMakePictureSnapshotProxy = SKManagedDrawableMakePictureSnapshotProxyImplementation;

	public unsafe static readonly SKManagedStreamDestroyProxyDelegate SKManagedStreamDestroyProxy = SKManagedStreamDestroyProxyImplementation;

	public unsafe static readonly SKManagedStreamDuplicateProxyDelegate SKManagedStreamDuplicateProxy = SKManagedStreamDuplicateProxyImplementation;

	public unsafe static readonly SKManagedStreamForkProxyDelegate SKManagedStreamForkProxy = SKManagedStreamForkProxyImplementation;

	public unsafe static readonly SKManagedStreamGetLengthProxyDelegate SKManagedStreamGetLengthProxy = SKManagedStreamGetLengthProxyImplementation;

	public unsafe static readonly SKManagedStreamGetPositionProxyDelegate SKManagedStreamGetPositionProxy = SKManagedStreamGetPositionProxyImplementation;

	public unsafe static readonly SKManagedStreamHasLengthProxyDelegate SKManagedStreamHasLengthProxy = SKManagedStreamHasLengthProxyImplementation;

	public unsafe static readonly SKManagedStreamHasPositionProxyDelegate SKManagedStreamHasPositionProxy = SKManagedStreamHasPositionProxyImplementation;

	public unsafe static readonly SKManagedStreamIsAtEndProxyDelegate SKManagedStreamIsAtEndProxy = SKManagedStreamIsAtEndProxyImplementation;

	public unsafe static readonly SKManagedStreamMoveProxyDelegate SKManagedStreamMoveProxy = SKManagedStreamMoveProxyImplementation;

	public unsafe static readonly SKManagedStreamPeekProxyDelegate SKManagedStreamPeekProxy = SKManagedStreamPeekProxyImplementation;

	public unsafe static readonly SKManagedStreamReadProxyDelegate SKManagedStreamReadProxy = SKManagedStreamReadProxyImplementation;

	public unsafe static readonly SKManagedStreamRewindProxyDelegate SKManagedStreamRewindProxy = SKManagedStreamRewindProxyImplementation;

	public unsafe static readonly SKManagedStreamSeekProxyDelegate SKManagedStreamSeekProxy = SKManagedStreamSeekProxyImplementation;

	public unsafe static readonly SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate SKManagedTraceMemoryDumpDumpNumericValueProxy = SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation;

	public unsafe static readonly SKManagedTraceMemoryDumpDumpStringValueProxyDelegate SKManagedTraceMemoryDumpDumpStringValueProxy = SKManagedTraceMemoryDumpDumpStringValueProxyImplementation;

	public unsafe static readonly SKManagedWStreamBytesWrittenProxyDelegate SKManagedWStreamBytesWrittenProxy = SKManagedWStreamBytesWrittenProxyImplementation;

	public unsafe static readonly SKManagedWStreamDestroyProxyDelegate SKManagedWStreamDestroyProxy = SKManagedWStreamDestroyProxyImplementation;

	public unsafe static readonly SKManagedWStreamFlushProxyDelegate SKManagedWStreamFlushProxy = SKManagedWStreamFlushProxyImplementation;

	public unsafe static readonly SKManagedWStreamWriteProxyDelegate SKManagedWStreamWriteProxy = SKManagedWStreamWriteProxyImplementation;

	public unsafe static readonly SKSurfaceRasterReleaseProxyDelegate SKSurfaceRasterReleaseProxy = SKSurfaceRasterReleaseProxyImplementation;

	[MonoPInvokeCallback(typeof(GRGlGetProcProxyDelegate))]
	private unsafe static IntPtr GRGlGetProcProxyImplementation(void* ctx, void* name)
	{
		GRGlGetProcedureAddressDelegate gRGlGetProcedureAddressDelegate = Get<GRGlGetProcedureAddressDelegate>((IntPtr)ctx, out var _);
		return gRGlGetProcedureAddressDelegate(Marshal.PtrToStringAnsi((IntPtr)name));
	}

	[MonoPInvokeCallback(typeof(GRVkGetProcProxyDelegate))]
	private unsafe static IntPtr GRVkGetProcProxyImplementation(void* ctx, void* name, IntPtr instance, IntPtr device)
	{
		GRVkGetProcedureAddressDelegate gRVkGetProcedureAddressDelegate = Get<GRVkGetProcedureAddressDelegate>((IntPtr)ctx, out var _);
		return gRVkGetProcedureAddressDelegate(Marshal.PtrToStringAnsi((IntPtr)name), instance, device);
	}

	[MonoPInvokeCallback(typeof(SKBitmapReleaseProxyDelegate))]
	private unsafe static void SKBitmapReleaseProxyImplementation(void* addr, void* context)
	{
		SKBitmapReleaseDelegate sKBitmapReleaseDelegate = Get<SKBitmapReleaseDelegate>((IntPtr)context, out var gch);
		try
		{
			sKBitmapReleaseDelegate((IntPtr)addr, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKDataReleaseProxyDelegate))]
	private unsafe static void SKDataReleaseProxyImplementation(void* ptr, void* context)
	{
		SKDataReleaseDelegate sKDataReleaseDelegate = Get<SKDataReleaseDelegate>((IntPtr)context, out var gch);
		try
		{
			sKDataReleaseDelegate((IntPtr)ptr, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKGlyphPathProxyDelegate))]
	private unsafe static void SKGlyphPathProxyImplementation(IntPtr pathOrNull, SKMatrix* matrix, void* context)
	{
		SKGlyphPathDelegate sKGlyphPathDelegate = Get<SKGlyphPathDelegate>((IntPtr)context, out var _);
		SKPath path = SKPath.GetObject(pathOrNull, owns: false);
		sKGlyphPathDelegate(path, *matrix);
	}

	[MonoPInvokeCallback(typeof(SKImageRasterReleaseProxyDelegate))]
	private unsafe static void SKImageRasterReleaseProxyImplementation(void* addr, void* context)
	{
		SKImageRasterReleaseDelegate sKImageRasterReleaseDelegate = Get<SKImageRasterReleaseDelegate>((IntPtr)context, out var gch);
		try
		{
			sKImageRasterReleaseDelegate((IntPtr)addr, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKImageRasterReleaseProxyDelegate))]
	private unsafe static void SKImageRasterReleaseProxyImplementationForCoTaskMem(void* addr, void* context)
	{
		Marshal.FreeCoTaskMem((IntPtr)addr);
	}

	[MonoPInvokeCallback(typeof(SKImageTextureReleaseProxyDelegate))]
	private unsafe static void SKImageTextureReleaseProxyImplementation(void* context)
	{
		SKImageTextureReleaseDelegate sKImageTextureReleaseDelegate = Get<SKImageTextureReleaseDelegate>((IntPtr)context, out var gch);
		try
		{
			sKImageTextureReleaseDelegate(null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableApproximateBytesUsedProxyDelegate))]
	private unsafe static IntPtr SKManagedDrawableApproximateBytesUsedProxyImplementation(IntPtr d, void* context)
	{
		SKDrawable userData = GetUserData<SKDrawable>((IntPtr)context, out var _);
		return (IntPtr)userData.OnGetApproximateBytesUsed();
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableDestroyProxyDelegate))]
	private unsafe static void SKManagedDrawableDestroyProxyImplementation(IntPtr d, void* context)
	{
		SKDrawable userData = GetUserData<SKDrawable>((IntPtr)context, out var gch);
		if (userData != null)
		{
			Interlocked.Exchange(ref userData.fromNative, 1);
			userData.Dispose();
		}
		gch.Free();
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableDrawProxyDelegate))]
	private unsafe static void SKManagedDrawableDrawProxyImplementation(IntPtr d, void* context, IntPtr ccanvas)
	{
		SKDrawable userData = GetUserData<SKDrawable>((IntPtr)context, out var _);
		userData.OnDraw(SKCanvas.GetObject(ccanvas, owns: false));
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableGetBoundsProxyDelegate))]
	private unsafe static void SKManagedDrawableGetBoundsProxyImplementation(IntPtr d, void* context, SKRect* rect)
	{
		SKDrawable userData = GetUserData<SKDrawable>((IntPtr)context, out var _);
		SKRect sKRect = userData.OnGetBounds();
		*rect = sKRect;
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableMakePictureSnapshotProxyDelegate))]
	private unsafe static IntPtr SKManagedDrawableMakePictureSnapshotProxyImplementation(IntPtr d, void* context)
	{
		SKDrawable userData = GetUserData<SKDrawable>((IntPtr)context, out var _);
		return userData.OnSnapshot()?.Handle ?? IntPtr.Zero;
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamDestroyProxyDelegate))]
	private unsafe static void SKManagedStreamDestroyProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var gch);
		if (userData != null)
		{
			Interlocked.Exchange(ref userData.fromNative, 1);
			userData.Dispose();
		}
		gch.Free();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamDuplicateProxyDelegate))]
	private unsafe static IntPtr SKManagedStreamDuplicateProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnDuplicate();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamForkProxyDelegate))]
	private unsafe static IntPtr SKManagedStreamForkProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnFork();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamGetLengthProxyDelegate))]
	private unsafe static IntPtr SKManagedStreamGetLengthProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnGetLength();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamGetPositionProxyDelegate))]
	private unsafe static IntPtr SKManagedStreamGetPositionProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnGetPosition();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamHasLengthProxyDelegate))]
	[return: MarshalAs(UnmanagedType.I1)]
	private unsafe static bool SKManagedStreamHasLengthProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnHasLength();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamHasPositionProxyDelegate))]
	[return: MarshalAs(UnmanagedType.I1)]
	private unsafe static bool SKManagedStreamHasPositionProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnHasPosition();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamIsAtEndProxyDelegate))]
	[return: MarshalAs(UnmanagedType.I1)]
	private unsafe static bool SKManagedStreamIsAtEndProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnIsAtEnd();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamMoveProxyDelegate))]
	[return: MarshalAs(UnmanagedType.I1)]
	private unsafe static bool SKManagedStreamMoveProxyImplementation(IntPtr s, void* context, int offset)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnMove(offset);
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamPeekProxyDelegate))]
	private unsafe static IntPtr SKManagedStreamPeekProxyImplementation(IntPtr s, void* context, void* buffer, IntPtr size)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnPeek((IntPtr)buffer, size);
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamReadProxyDelegate))]
	private unsafe static IntPtr SKManagedStreamReadProxyImplementation(IntPtr s, void* context, void* buffer, IntPtr size)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnRead((IntPtr)buffer, size);
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamRewindProxyDelegate))]
	[return: MarshalAs(UnmanagedType.I1)]
	private unsafe static bool SKManagedStreamRewindProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnRewind();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamSeekProxyDelegate))]
	[return: MarshalAs(UnmanagedType.I1)]
	private unsafe static bool SKManagedStreamSeekProxyImplementation(IntPtr s, void* context, IntPtr position)
	{
		SKAbstractManagedStream userData = GetUserData<SKAbstractManagedStream>((IntPtr)context, out var _);
		return userData.OnSeek(position);
	}

	[MonoPInvokeCallback(typeof(SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate))]
	private unsafe static void SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation(IntPtr d, void* context, void* dumpName, void* valueName, void* units, ulong value)
	{
		SKTraceMemoryDump userData = GetUserData<SKTraceMemoryDump>((IntPtr)context, out var _);
		userData.OnDumpNumericValue(Marshal.PtrToStringAnsi((IntPtr)dumpName), Marshal.PtrToStringAnsi((IntPtr)valueName), Marshal.PtrToStringAnsi((IntPtr)units), value);
	}

	[MonoPInvokeCallback(typeof(SKManagedTraceMemoryDumpDumpStringValueProxyDelegate))]
	private unsafe static void SKManagedTraceMemoryDumpDumpStringValueProxyImplementation(IntPtr d, void* context, void* dumpName, void* valueName, void* value)
	{
		SKTraceMemoryDump userData = GetUserData<SKTraceMemoryDump>((IntPtr)context, out var _);
		userData.OnDumpStringValue(Marshal.PtrToStringAnsi((IntPtr)dumpName), Marshal.PtrToStringAnsi((IntPtr)valueName), Marshal.PtrToStringAnsi((IntPtr)value));
	}

	[MonoPInvokeCallback(typeof(SKManagedWStreamBytesWrittenProxyDelegate))]
	private unsafe static IntPtr SKManagedWStreamBytesWrittenProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedWStream userData = GetUserData<SKAbstractManagedWStream>((IntPtr)context, out var _);
		return userData.OnBytesWritten();
	}

	[MonoPInvokeCallback(typeof(SKManagedWStreamDestroyProxyDelegate))]
	private unsafe static void SKManagedWStreamDestroyProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedWStream userData = GetUserData<SKAbstractManagedWStream>((IntPtr)context, out var gch);
		if (userData != null)
		{
			Interlocked.Exchange(ref userData.fromNative, 1);
			userData.Dispose();
		}
		gch.Free();
	}

	[MonoPInvokeCallback(typeof(SKManagedWStreamFlushProxyDelegate))]
	private unsafe static void SKManagedWStreamFlushProxyImplementation(IntPtr s, void* context)
	{
		SKAbstractManagedWStream userData = GetUserData<SKAbstractManagedWStream>((IntPtr)context, out var _);
		userData.OnFlush();
	}

	[MonoPInvokeCallback(typeof(SKManagedWStreamWriteProxyDelegate))]
	[return: MarshalAs(UnmanagedType.I1)]
	private unsafe static bool SKManagedWStreamWriteProxyImplementation(IntPtr s, void* context, void* buffer, IntPtr size)
	{
		SKAbstractManagedWStream userData = GetUserData<SKAbstractManagedWStream>((IntPtr)context, out var _);
		return userData.OnWrite((IntPtr)buffer, size);
	}

	[MonoPInvokeCallback(typeof(SKSurfaceRasterReleaseProxyDelegate))]
	private unsafe static void SKSurfaceRasterReleaseProxyImplementation(void* addr, void* context)
	{
		SKSurfaceReleaseDelegate sKSurfaceReleaseDelegate = Get<SKSurfaceReleaseDelegate>((IntPtr)context, out var gch);
		try
		{
			sKSurfaceReleaseDelegate((IntPtr)addr, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Create(object managedDel, out GCHandle gch, out IntPtr contextPtr)
	{
		if (managedDel == null)
		{
			gch = default(GCHandle);
			contextPtr = IntPtr.Zero;
		}
		else
		{
			gch = GCHandle.Alloc(managedDel);
			contextPtr = GCHandle.ToIntPtr(gch);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Get<T>(IntPtr contextPtr, out GCHandle gch)
	{
		if (contextPtr == IntPtr.Zero)
		{
			gch = default(GCHandle);
			return default(T);
		}
		gch = GCHandle.FromIntPtr(contextPtr);
		return (T)gch.Target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateUserData(object userData, bool makeWeak = false)
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate managedDel = () => userData;
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetUserData<T>(IntPtr contextPtr, out GCHandle gch)
	{
		UserDataDelegate userDataDelegate = Get<UserDataDelegate>(contextPtr, out gch);
		object obj = userDataDelegate();
		if (!(obj is WeakReference weakReference))
		{
			return (T)obj;
		}
		return (T)weakReference.Target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMulti<T1, T2>(T1 wrappedDelegate1, T2 wrappedDelegate2) where T1 : Delegate where T2 : Delegate
	{
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMulti<T1, T2, T3>(T1 wrappedDelegate1, T2 wrappedDelegate2, T3 wrappedDelegate3) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			if (type == typeof(T3))
			{
				return wrappedDelegate3;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetMulti<T>(IntPtr contextPtr, out GCHandle gch) where T : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		return (T)getMultiDelegateDelegate(typeof(T));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMulti<T1, T2>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out GCHandle gch) where T1 : Delegate where T2 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMulti<T1, T2, T3>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out T3 wrappedDelegate3, out GCHandle gch) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
		wrappedDelegate3 = (T3)getMultiDelegateDelegate(typeof(T3));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMultiUserData<T>(T wrappedDelegate, object userData, bool makeWeak = false) where T : Delegate
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate userDataDelegate = () => userData;
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T))
			{
				return wrappedDelegate;
			}
			if (type == typeof(UserDataDelegate))
			{
				return userDataDelegate;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMultiUserData<T1, T2>(T1 wrappedDelegate1, T2 wrappedDelegate2, object userData, bool makeWeak = false) where T1 : Delegate where T2 : Delegate
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate userDataDelegate = () => userData;
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			if (type == typeof(UserDataDelegate))
			{
				return userDataDelegate;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMultiUserData<T1, T2, T3>(T1 wrappedDelegate1, T2 wrappedDelegate2, T3 wrappedDelegate3, object userData, bool makeWeak = false) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate userDataDelegate = () => userData;
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			if (type == typeof(T3))
			{
				return wrappedDelegate3;
			}
			if (type == typeof(UserDataDelegate))
			{
				return userDataDelegate;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TUserData GetMultiUserData<TUserData>(IntPtr contextPtr, out GCHandle gch)
	{
		GetMultiDelegateDelegate multi = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		return GetUserData<TUserData>(multi);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMultiUserData<T, TUserData>(IntPtr contextPtr, out T wrappedDelegate, out TUserData userData, out GCHandle gch) where T : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate = (T)getMultiDelegateDelegate(typeof(T));
		userData = GetUserData<TUserData>(getMultiDelegateDelegate);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMultiUserData<T1, T2, TUserData>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out TUserData userData, out GCHandle gch) where T1 : Delegate where T2 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
		userData = GetUserData<TUserData>(getMultiDelegateDelegate);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMultiUserData<T1, T2, T3, TUserData>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out T3 wrappedDelegate3, out TUserData userData, out GCHandle gch) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
		wrappedDelegate3 = (T3)getMultiDelegateDelegate(typeof(T3));
		userData = GetUserData<TUserData>(getMultiDelegateDelegate);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static TUserData GetUserData<TUserData>(GetMultiDelegateDelegate multi)
	{
		UserDataDelegate userDataDelegate = (UserDataDelegate)multi(typeof(UserDataDelegate));
		object obj = userDataDelegate();
		if (!(obj is WeakReference weakReference))
		{
			return (TUserData)obj;
		}
		return (TUserData)weakReference.Target;
	}
}

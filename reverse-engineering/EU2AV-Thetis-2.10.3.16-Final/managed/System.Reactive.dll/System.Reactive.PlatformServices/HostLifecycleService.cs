using System.ComponentModel;
using System.Threading;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HostLifecycleService
{
	private static readonly Lazy<IHostLifecycleNotifications?> Notifications = new Lazy<IHostLifecycleNotifications>(InitializeNotifications);

	private static int _refCount;

	public static event EventHandler<HostSuspendingEventArgs>? Suspending;

	public static event EventHandler<HostResumingEventArgs>? Resuming;

	public static void AddRef()
	{
		if (Interlocked.Increment(ref _refCount) == 1)
		{
			IHostLifecycleNotifications value = Notifications.Value;
			if (value != null)
			{
				value.Suspending += OnSuspending;
				value.Resuming += OnResuming;
			}
		}
	}

	public static void Release()
	{
		if (Interlocked.Decrement(ref _refCount) == 0)
		{
			IHostLifecycleNotifications value = Notifications.Value;
			if (value != null)
			{
				value.Suspending -= OnSuspending;
				value.Resuming -= OnResuming;
			}
		}
	}

	private static void OnSuspending(object? sender, HostSuspendingEventArgs e)
	{
		Suspending?.Invoke(sender, e);
	}

	private static void OnResuming(object? sender, HostResumingEventArgs e)
	{
		Resuming?.Invoke(sender, e);
	}

	private static IHostLifecycleNotifications? InitializeNotifications()
	{
		return PlatformEnlightenmentProvider.Current.GetService<IHostLifecycleNotifications>(Array.Empty<object>());
	}
}

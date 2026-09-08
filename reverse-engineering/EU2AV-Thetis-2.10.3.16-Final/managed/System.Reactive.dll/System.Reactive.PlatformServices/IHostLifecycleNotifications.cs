using System.ComponentModel;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IHostLifecycleNotifications
{
	event EventHandler<HostSuspendingEventArgs> Suspending;

	event EventHandler<HostResumingEventArgs> Resuming;
}

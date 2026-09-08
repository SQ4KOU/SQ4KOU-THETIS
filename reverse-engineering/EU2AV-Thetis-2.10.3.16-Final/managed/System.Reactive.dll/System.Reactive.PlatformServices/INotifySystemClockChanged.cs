using System.ComponentModel;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface INotifySystemClockChanged
{
	event EventHandler<SystemClockChangedEventArgs> SystemClockChanged;
}

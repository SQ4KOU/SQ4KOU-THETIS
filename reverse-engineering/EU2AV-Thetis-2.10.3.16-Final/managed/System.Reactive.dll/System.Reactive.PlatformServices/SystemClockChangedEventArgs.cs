using System.ComponentModel;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public class SystemClockChangedEventArgs : EventArgs
{
	public DateTimeOffset OldTime { get; }

	public DateTimeOffset NewTime { get; }

	public SystemClockChangedEventArgs()
		: this(DateTimeOffset.MinValue, DateTimeOffset.MaxValue)
	{
	}

	public SystemClockChangedEventArgs(DateTimeOffset oldTime, DateTimeOffset newTime)
	{
		OldTime = oldTime;
		NewTime = newTime;
	}
}

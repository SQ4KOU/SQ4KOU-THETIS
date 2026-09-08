using System.ComponentModel;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DefaultSystemClock : ISystemClock
{
	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

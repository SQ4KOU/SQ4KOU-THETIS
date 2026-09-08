using System.ComponentModel;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface ISystemClock
{
	DateTimeOffset UtcNow { get; }
}

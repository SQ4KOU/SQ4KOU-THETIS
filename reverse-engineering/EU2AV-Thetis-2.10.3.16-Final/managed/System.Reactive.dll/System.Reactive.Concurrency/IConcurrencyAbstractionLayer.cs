using System.ComponentModel;

namespace System.Reactive.Concurrency;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IConcurrencyAbstractionLayer
{
	bool SupportsLongRunning { get; }

	IDisposable StartTimer(Action<object?> action, object? state, TimeSpan dueTime);

	IDisposable StartPeriodicTimer(Action action, TimeSpan period);

	IDisposable QueueUserWorkItem(Action<object?> action, object? state);

	void Sleep(TimeSpan timeout);

	IStopwatch StartStopwatch();

	void StartThread(Action<object?> action, object? state);
}

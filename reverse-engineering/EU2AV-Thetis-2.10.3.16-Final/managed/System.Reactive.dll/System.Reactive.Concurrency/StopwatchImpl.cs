using System.Diagnostics;

namespace System.Reactive.Concurrency;

internal class StopwatchImpl : IStopwatch
{
	private readonly Stopwatch _sw;

	public TimeSpan Elapsed => _sw.Elapsed;

	public StopwatchImpl()
	{
		_sw = Stopwatch.StartNew();
	}
}

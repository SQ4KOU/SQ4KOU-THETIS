using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public class PeriodicTimerSystemClockMonitor : INotifySystemClockChanged
{
	private readonly TimeSpan _period;

	private SerialDisposableValue _timer;

	private long _lastTimeUnixMillis;

	private EventHandler<SystemClockChangedEventArgs>? _systemClockChanged;

	private const int SyncMaxRetries = 100;

	private const double SyncMaxDelta = 10.0;

	private const int MaxError = 100;

	public event EventHandler<SystemClockChangedEventArgs> SystemClockChanged
	{
		add
		{
			NewTimer();
			_systemClockChanged = (EventHandler<SystemClockChangedEventArgs>)Delegate.Combine(_systemClockChanged, value);
		}
		remove
		{
			_systemClockChanged = (EventHandler<SystemClockChangedEventArgs>)Delegate.Remove(_systemClockChanged, value);
			_timer.Disposable = Disposable.Empty;
		}
	}

	public PeriodicTimerSystemClockMonitor(TimeSpan period)
	{
		_period = period;
	}

	private void NewTimer()
	{
		_timer.Disposable = Disposable.Empty;
		long num = 0L;
		while (true)
		{
			long num2 = SystemClock.UtcNow.ToUnixTimeMilliseconds();
			Interlocked.Exchange(ref _lastTimeUnixMillis, num2);
			_timer.Disposable = ConcurrencyAbstractionLayer.Current.StartPeriodicTimer(TimeChanged, _period);
			if (!((double)Math.Abs(SystemClock.UtcNow.ToUnixTimeMilliseconds() - num2) <= 10.0) && _timer.Disposable != Disposable.Empty)
			{
				if (++num >= 100)
				{
					Task.Delay(10).Wait();
				}
				continue;
			}
			break;
		}
	}

	private void TimeChanged()
	{
		DateTimeOffset utcNow = SystemClock.UtcNow;
		long num = utcNow.ToUnixTimeMilliseconds();
		long num2 = (long)((double)Volatile.Read(ref _lastTimeUnixMillis) + _period.TotalMilliseconds);
		if (Math.Abs(num - num2) >= 100)
		{
			_systemClockChanged?.Invoke(this, new SystemClockChangedEventArgs(DateTimeOffset.FromUnixTimeMilliseconds(num2), utcNow));
			NewTimer();
		}
		else
		{
			Interlocked.Exchange(ref _lastTimeUnixMillis, SystemClock.UtcNow.ToUnixTimeMilliseconds());
		}
	}
}

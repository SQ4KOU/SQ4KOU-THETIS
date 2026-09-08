using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Concurrency;

public sealed class NewThreadScheduler : LocalScheduler, ISchedulerLongRunning, ISchedulerPeriodic
{
	private sealed class Periodic<TState> : IDisposable
	{
		private readonly IStopwatch _stopwatch;

		private readonly TimeSpan _period;

		private readonly Func<TState, TState> _action;

		private readonly object _cancel = new object();

		private volatile bool _done;

		private TState _state;

		private TimeSpan _next;

		public Periodic(TState state, TimeSpan period, Func<TState, TState> action)
		{
			_stopwatch = ConcurrencyAbstractionLayer.Current.StartStopwatch();
			_period = period;
			_action = action;
			_state = state;
			_next = period;
		}

		public void Run()
		{
			while (!_done)
			{
				TimeSpan timeout = Scheduler.Normalize(_next - _stopwatch.Elapsed);
				lock (_cancel)
				{
					if (Monitor.Wait(_cancel, timeout))
					{
						break;
					}
				}
				_state = _action(_state);
				_next += _period;
			}
		}

		public void Dispose()
		{
			_done = true;
			lock (_cancel)
			{
				Monitor.Pulse(_cancel);
			}
		}
	}

	private static readonly Lazy<NewThreadScheduler> Instance = new Lazy<NewThreadScheduler>(() => new NewThreadScheduler());

	private readonly Func<ThreadStart, Thread> _threadFactory;

	public static NewThreadScheduler Default => Instance.Value;

	public NewThreadScheduler()
		: this((ThreadStart action) => new Thread(action))
	{
	}

	public NewThreadScheduler(Func<ThreadStart, Thread> threadFactory)
	{
		_threadFactory = threadFactory ?? throw new ArgumentNullException("threadFactory");
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return new EventLoopScheduler(_threadFactory)
		{
			ExitIfEmpty = true
		}.Schedule(state, dueTime, action);
	}

	public IDisposable ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		BooleanDisposable d = new BooleanDisposable();
		_threadFactory(delegate
		{
			action(state, d);
		}).Start();
		return d;
	}

	public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
	{
		if (period < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		Periodic<TState> periodic = new Periodic<TState>(state, period, action);
		_threadFactory(periodic.Run).Start();
		return periodic;
	}

	public override IStopwatch StartStopwatch()
	{
		return new StopwatchImpl();
	}
}

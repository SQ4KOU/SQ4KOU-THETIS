using System.Reactive.Disposables;

namespace System.Reactive.Concurrency;

public sealed class ImmediateScheduler : LocalScheduler
{
	private sealed class AsyncLockScheduler : LocalScheduler
	{
		private AsyncLock? _asyncLock;

		public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			if (_asyncLock == null)
			{
				_asyncLock = new AsyncLock();
			}
			_asyncLock.Wait((this, singleAssignmentDisposable, action, state), delegate((AsyncLockScheduler @this, SingleAssignmentDisposable m, Func<IScheduler, TState, IDisposable> action, TState state) tuple)
			{
				if (!tuple.m.IsDisposed)
				{
					tuple.m.Disposable = tuple.action(tuple.@this, tuple.state);
				}
			});
			return singleAssignmentDisposable;
		}

		public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			if (dueTime.Ticks <= 0)
			{
				return Schedule(state, action);
			}
			return ScheduleSlow(state, dueTime, action);
		}

		private IDisposable ScheduleSlow<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
		{
			IStopwatch item = ConcurrencyAbstractionLayer.Current.StartStopwatch();
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			if (_asyncLock == null)
			{
				_asyncLock = new AsyncLock();
			}
			_asyncLock.Wait((this, singleAssignmentDisposable, state, action, item, dueTime), delegate((AsyncLockScheduler @this, SingleAssignmentDisposable m, TState state, Func<IScheduler, TState, IDisposable> action, IStopwatch timer, TimeSpan dueTime) tuple)
			{
				if (!tuple.m.IsDisposed)
				{
					TimeSpan timeout = tuple.dueTime - tuple.timer.Elapsed;
					if (timeout.Ticks > 0)
					{
						ConcurrencyAbstractionLayer.Current.Sleep(timeout);
					}
					if (!tuple.m.IsDisposed)
					{
						tuple.m.Disposable = tuple.action(tuple.@this, tuple.state);
					}
				}
			});
			return singleAssignmentDisposable;
		}
	}

	private static readonly Lazy<ImmediateScheduler> StaticInstance = new Lazy<ImmediateScheduler>(() => new ImmediateScheduler());

	public static ImmediateScheduler Instance => StaticInstance.Value;

	private ImmediateScheduler()
	{
	}

	public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return action(new AsyncLockScheduler(), state);
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		TimeSpan timeout = Scheduler.Normalize(dueTime);
		if (timeout.Ticks > 0)
		{
			ConcurrencyAbstractionLayer.Current.Sleep(timeout);
		}
		return action(new AsyncLockScheduler(), state);
	}
}

using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Concurrency;

internal sealed class ConcurrencyAbstractionLayerImpl : IConcurrencyAbstractionLayer
{
	private sealed class WorkItem
	{
		public Action<object?> Action { get; }

		public object? State { get; }

		public WorkItem(Action<object?> action, object? state)
		{
			Action = action;
			State = state;
		}
	}

	private sealed class Timer : IDisposable
	{
		private volatile object? _state;

		private Action<object?> _action;

		private SingleAssignmentDisposableValue _timer;

		private static readonly object DisposedState = new object();

		public Timer(Action<object?> action, object? state, TimeSpan dueTime)
		{
			_state = state;
			_action = action;
			_timer.Disposable = new System.Threading.Timer(delegate(object @this)
			{
				((Timer)@this).Tick();
			}, this, dueTime, TimeSpan.FromMilliseconds(-1.0));
		}

		private void Tick()
		{
			try
			{
				object state = _state;
				if (state != DisposedState)
				{
					_action(state);
				}
			}
			finally
			{
				_timer.Dispose();
			}
		}

		public void Dispose()
		{
			_timer.Dispose();
			_action = Stubs<object>.Ignore;
			_state = DisposedState;
		}
	}

	private sealed class PeriodicTimer : IDisposable
	{
		private Action _action;

		private volatile System.Threading.Timer? _timer;

		public PeriodicTimer(Action action, TimeSpan period)
		{
			_action = action;
			_timer = new System.Threading.Timer(delegate(object @this)
			{
				((PeriodicTimer)@this).Tick();
			}, this, period, period);
		}

		private void Tick()
		{
			_action();
		}

		public void Dispose()
		{
			System.Threading.Timer timer = _timer;
			if (timer != null)
			{
				_action = Stubs.Nop;
				_timer = null;
				timer.Dispose();
			}
		}
	}

	private sealed class FastPeriodicTimer : IDisposable
	{
		private readonly Action _action;

		private volatile bool _disposed;

		public FastPeriodicTimer(Action action)
		{
			_action = action;
			Thread thread = new Thread(delegate(object @this)
			{
				((FastPeriodicTimer)@this).Loop();
			});
			thread.Name = "Rx-FastPeriodicTimer";
			thread.IsBackground = true;
			thread.Start(this);
		}

		private void Loop()
		{
			while (!_disposed)
			{
				_action();
			}
		}

		public void Dispose()
		{
			_disposed = true;
		}
	}

	public bool SupportsLongRunning => true;

	public IDisposable StartTimer(Action<object?> action, object? state, TimeSpan dueTime)
	{
		return new Timer(action, state, Normalize(dueTime));
	}

	public IDisposable StartPeriodicTimer(Action action, TimeSpan period)
	{
		if (period < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (period == TimeSpan.Zero)
		{
			return new FastPeriodicTimer(action);
		}
		return new PeriodicTimer(action, period);
	}

	public IDisposable QueueUserWorkItem(Action<object?> action, object? state)
	{
		ThreadPool.QueueUserWorkItem(delegate(object itemObject)
		{
			WorkItem workItem = (WorkItem)itemObject;
			workItem.Action(workItem.State);
		}, new WorkItem(action, state));
		return Disposable.Empty;
	}

	public void Sleep(TimeSpan timeout)
	{
		Thread.Sleep(Normalize(timeout));
	}

	public IStopwatch StartStopwatch()
	{
		return new StopwatchImpl();
	}

	public void StartThread(Action<object?> action, object? state)
	{
		Thread thread = new Thread(delegate(object itemObject)
		{
			WorkItem workItem = (WorkItem)itemObject;
			workItem.Action(workItem.State);
		});
		thread.IsBackground = true;
		thread.Start(new WorkItem(action, state));
	}

	private static TimeSpan Normalize(TimeSpan dueTime)
	{
		if (!(dueTime < TimeSpan.Zero))
		{
			return dueTime;
		}
		return TimeSpan.Zero;
	}
}

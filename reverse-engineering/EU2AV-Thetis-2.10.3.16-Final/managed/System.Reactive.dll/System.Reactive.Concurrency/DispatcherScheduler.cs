using System.Reactive.Disposables;
using System.Threading;
using System.Windows.Threading;

namespace System.Reactive.Concurrency;

public class DispatcherScheduler : LocalScheduler, ISchedulerPeriodic
{
	[Obsolete("Use the Current property to retrieve the DispatcherScheduler instance for the current thread's Dispatcher object.")]
	public static DispatcherScheduler Instance => new DispatcherScheduler(System.Windows.Threading.Dispatcher.CurrentDispatcher);

	public static DispatcherScheduler Current => new DispatcherScheduler(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread) ?? throw new InvalidOperationException(Strings_WindowsThreading.NO_DISPATCHER_CURRENT_THREAD));

	public Dispatcher Dispatcher { get; }

	public DispatcherPriority Priority { get; }

	public DispatcherScheduler(Dispatcher dispatcher)
	{
		Dispatcher = dispatcher ?? throw new ArgumentNullException("dispatcher");
		Priority = DispatcherPriority.Normal;
	}

	public DispatcherScheduler(Dispatcher dispatcher, DispatcherPriority priority)
	{
		Dispatcher = dispatcher ?? throw new ArgumentNullException("dispatcher");
		Priority = priority;
	}

	public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		SingleAssignmentDisposable d = new SingleAssignmentDisposable();
		Dispatcher.BeginInvoke((Action)delegate
		{
			if (!d.IsDisposed)
			{
				d.Disposable = action(this, state);
			}
		}, Priority);
		return d;
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		TimeSpan dueTime2 = Scheduler.Normalize(dueTime);
		if (dueTime2.Ticks == 0L)
		{
			return Schedule(state, action);
		}
		return ScheduleSlow(state, dueTime2, action);
	}

	private IDisposable ScheduleSlow<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		MultipleAssignmentDisposable d = new MultipleAssignmentDisposable();
		DispatcherTimer timer = new DispatcherTimer(Priority, Dispatcher);
		timer.Tick += delegate
		{
			DispatcherTimer dispatcherTimer = Interlocked.Exchange(ref timer, null);
			if (dispatcherTimer != null)
			{
				try
				{
					d.Disposable = action(this, state);
				}
				finally
				{
					dispatcherTimer.Stop();
					action = (IScheduler scheduler, TState t) => Disposable.Empty;
				}
			}
		};
		timer.Interval = dueTime;
		timer.Start();
		d.Disposable = Disposable.Create(delegate
		{
			DispatcherTimer dispatcherTimer = Interlocked.Exchange(ref timer, null);
			if (dispatcherTimer != null)
			{
				dispatcherTimer.Stop();
				action = (IScheduler s, TState t) => Disposable.Empty;
			}
		});
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
		DispatcherTimer timer = new DispatcherTimer(Priority, Dispatcher);
		TState state2 = state;
		timer.Tick += delegate
		{
			state2 = action(state2);
		};
		timer.Interval = period;
		timer.Start();
		return Disposable.Create(delegate
		{
			DispatcherTimer dispatcherTimer = Interlocked.Exchange(ref timer, null);
			if (dispatcherTimer != null)
			{
				dispatcherTimer.Stop();
				action = (TState _) => _;
			}
		});
	}
}

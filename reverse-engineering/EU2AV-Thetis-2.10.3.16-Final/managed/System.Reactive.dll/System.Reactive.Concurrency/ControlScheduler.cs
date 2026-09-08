using System.Reactive.Disposables;
using System.Threading;
using System.Windows.Forms;

namespace System.Reactive.Concurrency;

public class ControlScheduler : LocalScheduler, ISchedulerPeriodic
{
	private readonly Control _control;

	public Control Control => _control;

	public ControlScheduler(Control control)
	{
		_control = control ?? throw new ArgumentNullException("control");
	}

	public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (_control.IsDisposed)
		{
			return Disposable.Empty;
		}
		SingleAssignmentDisposable d = new SingleAssignmentDisposable();
		_control.BeginInvoke((Action)delegate
		{
			if (!_control.IsDisposed && !d.IsDisposed)
			{
				d.Disposable = action(this, state);
			}
		});
		return d;
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		TimeSpan dt = Scheduler.Normalize(dueTime);
		if (dt.Ticks == 0L)
		{
			return Schedule(state, action);
		}
		Func<IScheduler, TState, IDisposable> func = delegate(IScheduler scheduler1, TState arg)
		{
			MultipleAssignmentDisposable d = new MultipleAssignmentDisposable();
			System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
			timer.Tick += delegate
			{
				System.Windows.Forms.Timer timer2 = Interlocked.Exchange(ref timer, null);
				if (timer2 != null)
				{
					try
					{
						if (!_control.IsDisposed && !d.IsDisposed)
						{
							d.Disposable = action(scheduler1, arg);
						}
					}
					finally
					{
						timer2.Stop();
						action = (IScheduler scheduler2, TState t) => Disposable.Empty;
					}
				}
			};
			timer.Interval = (int)dt.TotalMilliseconds;
			timer.Start();
			d.Disposable = Disposable.Create(delegate
			{
				System.Windows.Forms.Timer timer2 = Interlocked.Exchange(ref timer, null);
				if (timer2 != null)
				{
					timer2.Stop();
					action = (IScheduler s, TState t) => Disposable.Empty;
				}
			});
			return d;
		};
		if (_control.InvokeRequired)
		{
			return Schedule(state, func);
		}
		return func(this, state);
	}

	public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
	{
		if (period.TotalMilliseconds < 1.0)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		Func<IScheduler, TState, IDisposable> func = delegate(IScheduler scheduler1, TState arg)
		{
			System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
			timer.Tick += delegate
			{
				if (!_control.IsDisposed)
				{
					arg = action(arg);
				}
			};
			timer.Interval = (int)period.TotalMilliseconds;
			timer.Start();
			return Disposable.Create(delegate
			{
				System.Windows.Forms.Timer timer2 = Interlocked.Exchange(ref timer, null);
				if (timer2 != null)
				{
					timer2.Stop();
					action = (TState _) => _;
				}
			});
		};
		if (_control.InvokeRequired)
		{
			return Schedule(state, func);
		}
		return func(this, state);
	}
}

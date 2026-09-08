using System.Collections.Generic;
using System.Globalization;

namespace System.Reactive.Concurrency;

public abstract class VirtualTimeSchedulerBase<TAbsolute, TRelative> : IScheduler, IServiceProvider, IStopwatchProvider where TAbsolute : IComparable<TAbsolute>
{
	private sealed class VirtualTimeStopwatch : IStopwatch
	{
		private readonly VirtualTimeSchedulerBase<TAbsolute, TRelative> _parent;

		private readonly DateTimeOffset _start;

		public TimeSpan Elapsed => _parent.ClockToDateTimeOffset() - _start;

		public VirtualTimeStopwatch(VirtualTimeSchedulerBase<TAbsolute, TRelative> parent, DateTimeOffset start)
		{
			_parent = parent;
			_start = start;
		}
	}

	public bool IsEnabled { get; private set; }

	protected IComparer<TAbsolute> Comparer { get; }

	public TAbsolute Clock { get; protected set; }

	public DateTimeOffset Now => ToDateTimeOffset(Clock);

	protected VirtualTimeSchedulerBase()
		: this(default(TAbsolute), (IComparer<TAbsolute>)Comparer<TAbsolute>.Default)
	{
	}

	protected VirtualTimeSchedulerBase(TAbsolute initialClock, IComparer<TAbsolute> comparer)
	{
		Clock = initialClock;
		Comparer = comparer ?? throw new ArgumentNullException("comparer");
	}

	protected abstract TAbsolute Add(TAbsolute absolute, TRelative relative);

	protected abstract DateTimeOffset ToDateTimeOffset(TAbsolute absolute);

	protected abstract TRelative ToRelative(TimeSpan timeSpan);

	public abstract IDisposable ScheduleAbsolute<TState>(TState state, TAbsolute dueTime, Func<IScheduler, TState, IDisposable> action);

	public IDisposable ScheduleRelative<TState>(TState state, TRelative dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		TAbsolute dueTime2 = Add(Clock, dueTime);
		return ScheduleAbsolute(state, dueTime2, action);
	}

	public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAbsolute(state, Clock, action);
	}

	public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleRelative(state, ToRelative(dueTime), action);
	}

	public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleRelative(state, ToRelative(dueTime - Now), action);
	}

	public void Start()
	{
		if (IsEnabled)
		{
			return;
		}
		IsEnabled = true;
		do
		{
			IScheduledItem<TAbsolute> next = GetNext();
			if (next != null)
			{
				if (Comparer.Compare(next.DueTime, Clock) > 0)
				{
					Clock = next.DueTime;
				}
				next.Invoke();
			}
			else
			{
				IsEnabled = false;
			}
		}
		while (IsEnabled);
	}

	public void Stop()
	{
		IsEnabled = false;
	}

	public void AdvanceTo(TAbsolute time)
	{
		int num = Comparer.Compare(time, Clock);
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("time");
		}
		if (num == 0)
		{
			return;
		}
		if (!IsEnabled)
		{
			IsEnabled = true;
			do
			{
				IScheduledItem<TAbsolute> next = GetNext();
				if (next != null && Comparer.Compare(next.DueTime, time) <= 0)
				{
					if (Comparer.Compare(next.DueTime, Clock) > 0)
					{
						Clock = next.DueTime;
					}
					next.Invoke();
				}
				else
				{
					IsEnabled = false;
				}
			}
			while (IsEnabled);
			Clock = time;
			return;
		}
		throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings_Linq.CANT_ADVANCE_WHILE_RUNNING, "AdvanceTo"));
	}

	public void AdvanceBy(TRelative time)
	{
		TAbsolute val = Add(Clock, time);
		int num = Comparer.Compare(val, Clock);
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("time");
		}
		if (num != 0)
		{
			if (IsEnabled)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings_Linq.CANT_ADVANCE_WHILE_RUNNING, "AdvanceBy"));
			}
			AdvanceTo(val);
		}
	}

	public void Sleep(TRelative time)
	{
		TAbsolute val = Add(Clock, time);
		if (Comparer.Compare(val, Clock) < 0)
		{
			throw new ArgumentOutOfRangeException("time");
		}
		Clock = val;
	}

	protected abstract IScheduledItem<TAbsolute>? GetNext();

	object? IServiceProvider.GetService(Type serviceType)
	{
		return GetService(serviceType);
	}

	protected virtual object? GetService(Type serviceType)
	{
		if (serviceType == typeof(IStopwatchProvider))
		{
			return this;
		}
		return null;
	}

	public IStopwatch StartStopwatch()
	{
		DateTimeOffset start = ClockToDateTimeOffset();
		return new VirtualTimeStopwatch(this, start);
	}

	private DateTimeOffset ClockToDateTimeOffset()
	{
		return ToDateTimeOffset(Clock);
	}
}

using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Reactive.Concurrency;

public abstract class ScheduledItem<TAbsolute> : IScheduledItem<TAbsolute>, IComparable<ScheduledItem<TAbsolute>>, IDisposable where TAbsolute : IComparable<TAbsolute>
{
	private SingleAssignmentDisposableValue _disposable;

	private readonly IComparer<TAbsolute> _comparer;

	public TAbsolute DueTime { get; }

	public bool IsCanceled => _disposable.IsDisposed;

	protected ScheduledItem(TAbsolute dueTime, IComparer<TAbsolute> comparer)
	{
		DueTime = dueTime;
		_comparer = comparer ?? throw new ArgumentNullException("comparer");
	}

	public void Invoke()
	{
		if (!_disposable.IsDisposed)
		{
			_disposable.Disposable = InvokeCore();
		}
	}

	protected abstract IDisposable InvokeCore();

	public int CompareTo(ScheduledItem<TAbsolute>? other)
	{
		if ((object)other == null)
		{
			return 1;
		}
		return _comparer.Compare(DueTime, other.DueTime);
	}

	public static bool operator <(ScheduledItem<TAbsolute> left, ScheduledItem<TAbsolute> right)
	{
		return Comparer<ScheduledItem<TAbsolute>>.Default.Compare(left, right) < 0;
	}

	public static bool operator <=(ScheduledItem<TAbsolute> left, ScheduledItem<TAbsolute> right)
	{
		return Comparer<ScheduledItem<TAbsolute>>.Default.Compare(left, right) <= 0;
	}

	public static bool operator >(ScheduledItem<TAbsolute> left, ScheduledItem<TAbsolute> right)
	{
		return Comparer<ScheduledItem<TAbsolute>>.Default.Compare(left, right) > 0;
	}

	public static bool operator >=(ScheduledItem<TAbsolute> left, ScheduledItem<TAbsolute> right)
	{
		return Comparer<ScheduledItem<TAbsolute>>.Default.Compare(left, right) >= 0;
	}

	public static bool operator ==(ScheduledItem<TAbsolute>? left, ScheduledItem<TAbsolute>? right)
	{
		return (object)left == right;
	}

	public static bool operator !=(ScheduledItem<TAbsolute>? left, ScheduledItem<TAbsolute>? right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj)
	{
		return this == obj;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public void Cancel()
	{
		_disposable.Dispose();
	}

	void IDisposable.Dispose()
	{
		Cancel();
	}
}
public sealed class ScheduledItem<TAbsolute, TValue> : ScheduledItem<TAbsolute> where TAbsolute : IComparable<TAbsolute>
{
	private readonly IScheduler _scheduler;

	private readonly TValue _state;

	private readonly Func<IScheduler, TValue, IDisposable> _action;

	public ScheduledItem(IScheduler scheduler, TValue state, Func<IScheduler, TValue, IDisposable> action, TAbsolute dueTime, IComparer<TAbsolute> comparer)
		: base(dueTime, comparer)
	{
		_scheduler = scheduler ?? throw new ArgumentNullException("scheduler");
		_state = state;
		_action = action ?? throw new ArgumentNullException("action");
	}

	public ScheduledItem(IScheduler scheduler, TValue state, Func<IScheduler, TValue, IDisposable> action, TAbsolute dueTime)
		: this(scheduler, state, action, dueTime, (IComparer<TAbsolute>)Comparer<TAbsolute>.Default)
	{
	}

	protected override IDisposable InvokeCore()
	{
		return _action(_scheduler, _state);
	}
}

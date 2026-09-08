using System.Reactive.Disposables;

namespace System.Reactive.Concurrency;

public static class VirtualTimeSchedulerExtensions
{
	public static IDisposable ScheduleRelative<TAbsolute, TRelative>(this VirtualTimeSchedulerBase<TAbsolute, TRelative> scheduler, TRelative dueTime, Action action) where TAbsolute : IComparable<TAbsolute>
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.ScheduleRelative(action, dueTime, (IScheduler _, Action a) => Invoke(a));
	}

	public static IDisposable ScheduleAbsolute<TAbsolute, TRelative>(this VirtualTimeSchedulerBase<TAbsolute, TRelative> scheduler, TAbsolute dueTime, Action action) where TAbsolute : IComparable<TAbsolute>
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.ScheduleAbsolute(action, dueTime, (IScheduler _, Action a) => Invoke(a));
	}

	private static IDisposable Invoke(Action action)
	{
		action();
		return Disposable.Empty;
	}
}

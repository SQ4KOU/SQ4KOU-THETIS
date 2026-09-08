using System.Reactive.Concurrency;

namespace System.Reactive.Disposables;

public sealed class ScheduledDisposable : ICancelable, IDisposable
{
	private SingleAssignmentDisposableValue _disposable;

	public IScheduler Scheduler { get; }

	public IDisposable Disposable => _disposable.Disposable ?? System.Reactive.Disposables.Disposable.Empty;

	public bool IsDisposed => _disposable.IsDisposed;

	public ScheduledDisposable(IScheduler scheduler, IDisposable disposable)
	{
		Scheduler = scheduler ?? throw new ArgumentNullException("scheduler");
		_disposable.Disposable = disposable ?? throw new ArgumentNullException("disposable");
	}

	public void Dispose()
	{
		Scheduler.ScheduleAction(this, delegate(ScheduledDisposable scheduler)
		{
			scheduler._disposable.Dispose();
		});
	}
}

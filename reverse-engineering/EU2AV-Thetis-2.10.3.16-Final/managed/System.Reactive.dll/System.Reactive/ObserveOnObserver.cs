using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive;

internal sealed class ObserveOnObserver<T> : ScheduledObserver<T>
{
	private SingleAssignmentDisposableValue _run;

	public ObserveOnObserver(IScheduler scheduler, IObserver<T> observer)
		: base(scheduler, observer)
	{
	}

	public void Run(IObservable<T> source)
	{
		_run.Disposable = source.SubscribeSafe(this);
	}

	protected override void OnNextCore(T value)
	{
		base.OnNextCore(value);
		EnsureActive();
	}

	protected override void OnErrorCore(Exception exception)
	{
		base.OnErrorCore(exception);
		EnsureActive();
	}

	protected override void OnCompletedCore()
	{
		base.OnCompletedCore();
		EnsureActive();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			_run.Dispose();
		}
	}
}

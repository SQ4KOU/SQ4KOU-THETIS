using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive;

internal abstract class BasicProducer<TSource> : IProducer<TSource>, IObservable<TSource>
{
	public IDisposable Subscribe(IObserver<TSource> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return SubscribeRaw(observer, enableSafeguard: true);
	}

	public IDisposable SubscribeRaw(IObserver<TSource> observer, bool enableSafeguard)
	{
		ISafeObserver<TSource> safeObserver = null;
		if (enableSafeguard)
		{
			observer = (safeObserver = SafeObserver<TSource>.Wrap(observer));
		}
		IDisposable disposable;
		if (CurrentThreadScheduler.IsScheduleRequired)
		{
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			CurrentThreadScheduler.Instance.ScheduleAction((this, singleAssignmentDisposable, observer), ((BasicProducer<TSource> @this, SingleAssignmentDisposable runAssignable, IObserver<TSource> observer) tuple) => tuple.runAssignable.Disposable = tuple.@this.Run(tuple.observer));
			disposable = singleAssignmentDisposable;
		}
		else
		{
			disposable = Run(observer);
		}
		safeObserver?.SetResource(disposable);
		return disposable;
	}

	protected abstract IDisposable Run(IObserver<TSource> observer);
}

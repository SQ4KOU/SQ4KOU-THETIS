using System.Reactive.Concurrency;

namespace System.Reactive;

internal abstract class Producer<TTarget, TSink> : IProducer<TTarget>, IObservable<TTarget> where TSink : IDisposable
{
	public IDisposable Subscribe(IObserver<TTarget> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return SubscribeRaw(observer, enableSafeguard: true);
	}

	public IDisposable SubscribeRaw(IObserver<TTarget> observer, bool enableSafeguard)
	{
		ISafeObserver<TTarget> safeObserver = null;
		if (enableSafeguard)
		{
			observer = (safeObserver = SafeObserver<TTarget>.Wrap(observer));
		}
		TSink val = CreateSink(observer);
		safeObserver?.SetResource(val);
		if (CurrentThreadScheduler.IsScheduleRequired)
		{
			CurrentThreadScheduler.Instance.ScheduleAction((this, val), delegate((Producer<TTarget, TSink> @this, TSink sink) tuple)
			{
				tuple.@this.Run(tuple.sink);
			});
		}
		else
		{
			Run(val);
		}
		return val;
	}

	protected abstract void Run(TSink sink);

	protected abstract TSink CreateSink(IObserver<TTarget> observer);
}

using System.Reactive.Concurrency;

namespace System.Reactive;

public abstract class ObservableBase<T> : IObservable<T>
{
	public IDisposable Subscribe(IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		AutoDetachObserver<T> autoDetachObserver = new AutoDetachObserver<T>(observer);
		if (CurrentThreadScheduler.IsScheduleRequired)
		{
			((IScheduler)CurrentThreadScheduler.Instance).ScheduleAction(autoDetachObserver, (Action<AutoDetachObserver<T>>)ScheduledSubscribe);
		}
		else
		{
			try
			{
				autoDetachObserver.SetResource(SubscribeCore(autoDetachObserver));
			}
			catch (Exception error)
			{
				if (!autoDetachObserver.Fail(error))
				{
					throw;
				}
			}
		}
		return autoDetachObserver;
	}

	private void ScheduledSubscribe(AutoDetachObserver<T> autoDetachObserver)
	{
		try
		{
			autoDetachObserver.SetResource(SubscribeCore(autoDetachObserver));
		}
		catch (Exception error)
		{
			if (!autoDetachObserver.Fail(error))
			{
				throw;
			}
		}
	}

	protected abstract IDisposable SubscribeCore(IObserver<T> observer);
}

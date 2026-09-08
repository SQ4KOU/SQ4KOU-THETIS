using System.Reactive.Concurrency;
using System.Threading;

namespace System.Reactive;

public static class Observer
{
	private class AnonymousProgress<T> : IProgress<T>
	{
		private readonly Action<T> _progress;

		public AnonymousProgress(Action<T> progress)
		{
			_progress = progress;
		}

		public void Report(T value)
		{
			_progress(value);
		}
	}

	public static IObserver<T> ToObserver<T>(this Action<Notification<T>> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		return new AnonymousObserver<T>(delegate(T x)
		{
			handler(Notification.CreateOnNext(x));
		}, delegate(Exception exception)
		{
			handler(Notification.CreateOnError<T>(exception));
		}, delegate
		{
			handler(Notification.CreateOnCompleted<T>());
		});
	}

	public static Action<Notification<T>> ToNotifier<T>(this IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return delegate(Notification<T> n)
		{
			n.Accept(observer);
		};
	}

	public static IObserver<T> Create<T>(Action<T> onNext)
	{
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		return new AnonymousObserver<T>(onNext);
	}

	public static IObserver<T> Create<T>(Action<T> onNext, Action<Exception> onError)
	{
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		return new AnonymousObserver<T>(onNext, onError);
	}

	public static IObserver<T> Create<T>(Action<T> onNext, Action onCompleted)
	{
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		return new AnonymousObserver<T>(onNext, onCompleted);
	}

	public static IObserver<T> Create<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted)
	{
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		return new AnonymousObserver<T>(onNext, onError, onCompleted);
	}

	public static IObserver<T> AsObserver<T>(this IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return new AnonymousObserver<T>(observer.OnNext, observer.OnError, observer.OnCompleted);
	}

	public static IObserver<T> Checked<T>(this IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return new CheckedObserver<T>(observer);
	}

	public static IObserver<T> Synchronize<T>(IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return new SynchronizedObserver<T>(observer, new object());
	}

	public static IObserver<T> Synchronize<T>(IObserver<T> observer, bool preventReentrancy)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (preventReentrancy)
		{
			return new AsyncLockObserver<T>(observer, new AsyncLock());
		}
		return new SynchronizedObserver<T>(observer, new object());
	}

	public static IObserver<T> Synchronize<T>(IObserver<T> observer, object gate)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (gate == null)
		{
			throw new ArgumentNullException("gate");
		}
		return new SynchronizedObserver<T>(observer, gate);
	}

	public static IObserver<T> Synchronize<T>(IObserver<T> observer, AsyncLock asyncLock)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (asyncLock == null)
		{
			throw new ArgumentNullException("asyncLock");
		}
		return new AsyncLockObserver<T>(observer, asyncLock);
	}

	public static IObserver<T> NotifyOn<T>(this IObserver<T> observer, IScheduler scheduler)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new ObserveOnObserver<T>(scheduler, observer);
	}

	public static IObserver<T> NotifyOn<T>(this IObserver<T> observer, SynchronizationContext context)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return new ObserveOnObserver<T>(new SynchronizationContextScheduler(context), observer);
	}

	public static IProgress<T> ToProgress<T>(this IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return new AnonymousProgress<T>(observer.OnNext);
	}

	public static IProgress<T> ToProgress<T>(this IObserver<T> observer, IScheduler scheduler)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new AnonymousProgress<T>(new ObserveOnObserver<T>(scheduler, observer).OnNext);
	}

	public static IObserver<T> ToObserver<T>(this IProgress<T> progress)
	{
		if (progress == null)
		{
			throw new ArgumentNullException("progress");
		}
		return new AnonymousObserver<T>(progress.Report);
	}
}

using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;

namespace System;

public static class ObservableExtensions
{
	public static IDisposable Subscribe<T>(this IObservable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Subscribe(new AnonymousObserver<T>(Stubs<T>.Ignore, Stubs.Throw, Stubs.Nop));
	}

	public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		return source.Subscribe(new AnonymousObserver<T>(onNext, Stubs.Throw, Stubs.Nop));
	}

	public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		return source.Subscribe(new AnonymousObserver<T>(onNext, onError, Stubs.Nop));
	}

	public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		return source.Subscribe(new AnonymousObserver<T>(onNext, Stubs.Throw, onCompleted));
	}

	public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
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
		return source.Subscribe(new AnonymousObserver<T>(onNext, onError, onCompleted));
	}

	public static void Subscribe<T>(this IObservable<T> source, IObserver<T> observer, CancellationToken token)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		source.Subscribe_(observer, token);
	}

	public static void Subscribe<T>(this IObservable<T> source, CancellationToken token)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		source.Subscribe_(new AnonymousObserver<T>(Stubs<T>.Ignore, Stubs.Throw, Stubs.Nop), token);
	}

	public static void Subscribe<T>(this IObservable<T> source, Action<T> onNext, CancellationToken token)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		source.Subscribe_(new AnonymousObserver<T>(onNext, Stubs.Throw, Stubs.Nop), token);
	}

	public static void Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, CancellationToken token)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		source.Subscribe_(new AnonymousObserver<T>(onNext, onError, Stubs.Nop), token);
	}

	public static void Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted, CancellationToken token)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		source.Subscribe_(new AnonymousObserver<T>(onNext, Stubs.Throw, onCompleted), token);
	}

	public static void Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted, CancellationToken token)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
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
		source.Subscribe_(new AnonymousObserver<T>(onNext, onError, onCompleted), token);
	}

	private static void Subscribe_<T>(this IObservable<T> source, IObserver<T> observer, CancellationToken token)
	{
		if (token.CanBeCanceled)
		{
			if (!token.IsCancellationRequested)
			{
				ISafeObserver<T> safeObserver = SafeObserver<T>.Wrap(observer);
				IDisposable state = source.Subscribe(safeObserver);
				safeObserver.SetResource(token.Register(delegate(object obj)
				{
					((IDisposable)obj).Dispose();
				}, state));
			}
		}
		else
		{
			source.Subscribe(observer);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static IDisposable SubscribeSafe<T>(this IObservable<T> source, IObserver<T> observer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (source is ObservableBase<T>)
		{
			return source.Subscribe(observer);
		}
		if (source is IProducer<T> producer)
		{
			return producer.SubscribeRaw(observer, enableSafeguard: false);
		}
		IDisposable result = Disposable.Empty;
		try
		{
			result = source.Subscribe(observer);
		}
		catch (Exception error)
		{
			observer.OnError(error);
		}
		return result;
	}
}

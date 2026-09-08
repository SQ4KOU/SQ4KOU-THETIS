using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace System.Reactive.Subjects;

public sealed class Subject<T> : SubjectBase<T>
{
	private sealed class SubjectDisposable : IDisposable
	{
		private Subject<T> _subject;

		private volatile IObserver<T>? _observer;

		public IObserver<T>? Observer => _observer;

		public SubjectDisposable(Subject<T> subject, IObserver<T> observer)
		{
			_subject = subject;
			_observer = observer;
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _observer, null) != null)
			{
				_subject.Unsubscribe(this);
				_subject = null;
			}
		}
	}

	private SubjectDisposable[] _observers;

	private Exception? _exception;

	private static readonly SubjectDisposable[] Terminated = new SubjectDisposable[0];

	private static readonly SubjectDisposable[] Disposed = new SubjectDisposable[0];

	public override bool HasObservers => Volatile.Read(ref _observers).Length != 0;

	public override bool IsDisposed => Volatile.Read(ref _observers) == Disposed;

	public Subject()
	{
		_observers = Array.Empty<SubjectDisposable>();
	}

	private static void ThrowDisposed()
	{
		throw new ObjectDisposedException(string.Empty);
	}

	public override void OnCompleted()
	{
		while (true)
		{
			SubjectDisposable[] array = Volatile.Read(ref _observers);
			if (array == Disposed)
			{
				_exception = null;
				ThrowDisposed();
				break;
			}
			if (array == Terminated)
			{
				break;
			}
			if (Interlocked.CompareExchange(ref _observers, Terminated, array) == array)
			{
				SubjectDisposable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Observer?.OnCompleted();
				}
				break;
			}
		}
	}

	public override void OnError(Exception error)
	{
		if (error == null)
		{
			throw new ArgumentNullException("error");
		}
		while (true)
		{
			SubjectDisposable[] array = Volatile.Read(ref _observers);
			if (array == Disposed)
			{
				_exception = null;
				ThrowDisposed();
				break;
			}
			if (array == Terminated)
			{
				break;
			}
			_exception = error;
			if (Interlocked.CompareExchange(ref _observers, Terminated, array) == array)
			{
				SubjectDisposable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Observer?.OnError(error);
				}
				break;
			}
		}
	}

	public override void OnNext(T value)
	{
		SubjectDisposable[] array = Volatile.Read(ref _observers);
		if (array == Disposed)
		{
			_exception = null;
			ThrowDisposed();
			return;
		}
		SubjectDisposable[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Observer?.OnNext(value);
		}
	}

	public override IDisposable Subscribe(IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		SubjectDisposable subjectDisposable = null;
		while (true)
		{
			SubjectDisposable[] array = Volatile.Read(ref _observers);
			if (array == Disposed)
			{
				_exception = null;
				ThrowDisposed();
				break;
			}
			if (array == Terminated)
			{
				Exception exception = _exception;
				if (exception != null)
				{
					observer.OnError(exception);
				}
				else
				{
					observer.OnCompleted();
				}
				break;
			}
			if (subjectDisposable == null)
			{
				subjectDisposable = new SubjectDisposable(this, observer);
			}
			int num = array.Length;
			SubjectDisposable[] array2 = new SubjectDisposable[num + 1];
			Array.Copy(array, 0, array2, 0, num);
			array2[num] = subjectDisposable;
			if (Interlocked.CompareExchange(ref _observers, array2, array) == array)
			{
				return subjectDisposable;
			}
		}
		return Disposable.Empty;
	}

	private void Unsubscribe(SubjectDisposable observer)
	{
		SubjectDisposable[] array;
		SubjectDisposable[] array2;
		do
		{
			array = Volatile.Read(ref _observers);
			int num = array.Length;
			if (num != 0)
			{
				int num2 = Array.IndexOf<SubjectDisposable>(array, observer);
				if (num2 >= 0)
				{
					if (num == 1)
					{
						array2 = Array.Empty<SubjectDisposable>();
						continue;
					}
					array2 = new SubjectDisposable[num - 1];
					Array.Copy(array, 0, array2, 0, num2);
					Array.Copy(array, num2 + 1, array2, num2, num - num2 - 1);
					continue;
				}
				break;
			}
			break;
		}
		while (Interlocked.CompareExchange(ref _observers, array2, array) != array);
	}

	public override void Dispose()
	{
		Interlocked.Exchange(ref _observers, Disposed);
		_exception = null;
	}
}
public static class Subject
{
	private class AnonymousSubject<T, U> : ISubject<T, U>, IObserver<T>, IObservable<U>
	{
		private readonly IObserver<T> _observer;

		private readonly IObservable<U> _observable;

		public AnonymousSubject(IObserver<T> observer, IObservable<U> observable)
		{
			_observer = observer;
			_observable = observable;
		}

		public void OnCompleted()
		{
			_observer.OnCompleted();
		}

		public void OnError(Exception error)
		{
			if (error == null)
			{
				throw new ArgumentNullException("error");
			}
			_observer.OnError(error);
		}

		public void OnNext(T value)
		{
			_observer.OnNext(value);
		}

		public IDisposable Subscribe(IObserver<U> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
			}
			return _observable.Subscribe(observer);
		}
	}

	private sealed class AnonymousSubject<T> : AnonymousSubject<T, T>, ISubject<T>, ISubject<T, T>, IObserver<T>, IObservable<T>
	{
		public AnonymousSubject(IObserver<T> observer, IObservable<T> observable)
			: base(observer, observable)
		{
		}
	}

	public static ISubject<TSource, TResult> Create<TSource, TResult>(IObserver<TSource> observer, IObservable<TResult> observable)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (observable == null)
		{
			throw new ArgumentNullException("observable");
		}
		return new AnonymousSubject<TSource, TResult>(observer, observable);
	}

	public static ISubject<T> Create<T>(IObserver<T> observer, IObservable<T> observable)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		if (observable == null)
		{
			throw new ArgumentNullException("observable");
		}
		return new AnonymousSubject<T>(observer, observable);
	}

	public static ISubject<TSource, TResult> Synchronize<TSource, TResult>(ISubject<TSource, TResult> subject)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		return new AnonymousSubject<TSource, TResult>(Observer.Synchronize(subject), subject);
	}

	public static ISubject<TSource> Synchronize<TSource>(ISubject<TSource> subject)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		return new AnonymousSubject<TSource>(Observer.Synchronize(subject), subject);
	}

	public static ISubject<TSource, TResult> Synchronize<TSource, TResult>(ISubject<TSource, TResult> subject, IScheduler scheduler)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new AnonymousSubject<TSource, TResult>(Observer.Synchronize(subject), subject.ObserveOn(scheduler));
	}

	public static ISubject<TSource> Synchronize<TSource>(ISubject<TSource> subject, IScheduler scheduler)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new AnonymousSubject<TSource>(Observer.Synchronize(subject), subject.ObserveOn(scheduler));
	}
}

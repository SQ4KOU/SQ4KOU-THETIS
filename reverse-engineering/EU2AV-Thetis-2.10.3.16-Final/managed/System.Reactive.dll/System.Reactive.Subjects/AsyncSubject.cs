using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Reactive.Subjects;

public sealed class AsyncSubject<T> : SubjectBase<T>, INotifyCompletion
{
	private sealed class AsyncSubjectDisposable : IDisposable
	{
		private AsyncSubject<T> _subject;

		private volatile IObserver<T>? _observer;

		public IObserver<T>? Observer => _observer;

		public AsyncSubjectDisposable(AsyncSubject<T> subject, IObserver<T> observer)
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

	private sealed class AwaitObserver : IObserver<T>
	{
		private readonly SynchronizationContext? _context;

		private readonly Action _callback;

		public AwaitObserver(Action callback)
		{
			_context = SynchronizationContext.Current;
			_callback = callback;
		}

		public void OnCompleted()
		{
			InvokeOnOriginalContext();
		}

		public void OnError(Exception error)
		{
			InvokeOnOriginalContext();
		}

		public void OnNext(T value)
		{
		}

		private void InvokeOnOriginalContext()
		{
			if (_context != null)
			{
				_context.Post(delegate(object c)
				{
					((Action)c)();
				}, _callback);
			}
			else
			{
				_callback();
			}
		}
	}

	private sealed class BlockingObserver : IObserver<T>
	{
		private readonly ManualResetEventSlim _e;

		public BlockingObserver(ManualResetEventSlim e)
		{
			_e = e;
		}

		public void OnCompleted()
		{
			Done();
		}

		public void OnError(Exception error)
		{
			Done();
		}

		public void OnNext(T value)
		{
		}

		private void Done()
		{
			_e.Set();
		}
	}

	private AsyncSubjectDisposable[] _observers;

	private T? _value;

	private bool _hasValue;

	private Exception? _exception;

	private static readonly AsyncSubjectDisposable[] Terminated = new AsyncSubjectDisposable[0];

	private static readonly AsyncSubjectDisposable[] Disposed = new AsyncSubjectDisposable[0];

	public override bool HasObservers => Volatile.Read(ref _observers).Length != 0;

	public override bool IsDisposed => Volatile.Read(ref _observers) == Disposed;

	public bool IsCompleted => Volatile.Read(ref _observers) == Terminated;

	public AsyncSubject()
	{
		_observers = Array.Empty<AsyncSubjectDisposable>();
	}

	public override void OnCompleted()
	{
		while (true)
		{
			AsyncSubjectDisposable[] array = Volatile.Read(ref _observers);
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
			if (Interlocked.CompareExchange(ref _observers, Terminated, array) != array)
			{
				continue;
			}
			if (_hasValue)
			{
				T value = _value;
				AsyncSubjectDisposable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					IObserver<T> observer = array2[i].Observer;
					if (observer != null)
					{
						observer.OnNext(value);
						observer.OnCompleted();
					}
				}
			}
			else
			{
				AsyncSubjectDisposable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Observer?.OnCompleted();
				}
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
			AsyncSubjectDisposable[] array = Volatile.Read(ref _observers);
			if (array == Disposed)
			{
				_exception = null;
				_value = default(T);
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
				AsyncSubjectDisposable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Observer?.OnError(error);
				}
			}
		}
	}

	public override void OnNext(T value)
	{
		AsyncSubjectDisposable[] array = Volatile.Read(ref _observers);
		if (array == Disposed)
		{
			_value = default(T);
			_exception = null;
			ThrowDisposed();
		}
		else if (array != Terminated)
		{
			_value = value;
			_hasValue = true;
		}
	}

	public override IDisposable Subscribe(IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		AsyncSubjectDisposable asyncSubjectDisposable = null;
		while (true)
		{
			AsyncSubjectDisposable[] array = Volatile.Read(ref _observers);
			if (array == Disposed)
			{
				_value = default(T);
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
					break;
				}
				if (_hasValue)
				{
					observer.OnNext(_value);
				}
				observer.OnCompleted();
				break;
			}
			if (asyncSubjectDisposable == null)
			{
				asyncSubjectDisposable = new AsyncSubjectDisposable(this, observer);
			}
			int num = array.Length;
			AsyncSubjectDisposable[] array2 = new AsyncSubjectDisposable[num + 1];
			Array.Copy(array, 0, array2, 0, num);
			array2[num] = asyncSubjectDisposable;
			if (Interlocked.CompareExchange(ref _observers, array2, array) == array)
			{
				return asyncSubjectDisposable;
			}
		}
		return Disposable.Empty;
	}

	private void Unsubscribe(AsyncSubjectDisposable observer)
	{
		AsyncSubjectDisposable[] array;
		AsyncSubjectDisposable[] array2;
		do
		{
			array = Volatile.Read(ref _observers);
			int num = array.Length;
			if (num != 0)
			{
				int num2 = Array.IndexOf<AsyncSubjectDisposable>(array, observer);
				if (num2 >= 0)
				{
					if (num == 1)
					{
						array2 = Array.Empty<AsyncSubjectDisposable>();
						continue;
					}
					array2 = new AsyncSubjectDisposable[num - 1];
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

	private static void ThrowDisposed()
	{
		throw new ObjectDisposedException(string.Empty);
	}

	public override void Dispose()
	{
		if (Interlocked.Exchange(ref _observers, Disposed) != Disposed)
		{
			_exception = null;
			_value = default(T);
			_hasValue = false;
		}
	}

	public AsyncSubject<T> GetAwaiter()
	{
		return this;
	}

	public void OnCompleted(Action continuation)
	{
		if (continuation == null)
		{
			throw new ArgumentNullException("continuation");
		}
		Subscribe(new AwaitObserver(continuation));
	}

	public T GetResult()
	{
		if (Volatile.Read(ref _observers) != Terminated)
		{
			using ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim(initialState: false);
			Subscribe(new BlockingObserver(manualResetEventSlim));
			manualResetEventSlim.Wait();
		}
		_exception?.Throw();
		if (!_hasValue)
		{
			throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
		}
		return _value;
	}
}

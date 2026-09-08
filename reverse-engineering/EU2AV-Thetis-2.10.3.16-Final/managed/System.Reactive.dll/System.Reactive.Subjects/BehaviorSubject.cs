using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Subjects;

public sealed class BehaviorSubject<T> : SubjectBase<T>
{
	private sealed class Subscription : IDisposable
	{
		private BehaviorSubject<T> _subject;

		private IObserver<T>? _observer;

		public Subscription(BehaviorSubject<T> subject, IObserver<T> observer)
		{
			_subject = subject;
			_observer = observer;
		}

		public void Dispose()
		{
			IObserver<T> observer = Interlocked.Exchange(ref _observer, null);
			if (observer != null)
			{
				_subject.Unsubscribe(observer);
				_subject = null;
			}
		}
	}

	private readonly object _gate = new object();

	private ImmutableList<IObserver<T>> _observers;

	private bool _isStopped;

	private T _value;

	private Exception? _exception;

	private bool _isDisposed;

	public override bool HasObservers
	{
		get
		{
			ImmutableList<IObserver<T>> observers = _observers;
			if (observers == null)
			{
				return false;
			}
			return observers.Data.Length != 0;
		}
	}

	public override bool IsDisposed
	{
		get
		{
			lock (_gate)
			{
				return _isDisposed;
			}
		}
	}

	public T Value
	{
		get
		{
			lock (_gate)
			{
				CheckDisposed();
				_exception?.Throw();
				return _value;
			}
		}
	}

	public BehaviorSubject(T value)
	{
		_value = value;
		_observers = ImmutableList<IObserver<T>>.Empty;
	}

	public bool TryGetValue([MaybeNullWhen(false)] out T value)
	{
		lock (_gate)
		{
			if (_isDisposed)
			{
				value = default(T);
				return false;
			}
			_exception?.Throw();
			value = _value;
			return true;
		}
	}

	public override void OnCompleted()
	{
		IObserver<T>[] array = null;
		lock (_gate)
		{
			CheckDisposed();
			if (!_isStopped)
			{
				array = _observers.Data;
				_observers = ImmutableList<IObserver<T>>.Empty;
				_isStopped = true;
			}
		}
		if (array != null)
		{
			IObserver<T>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnCompleted();
			}
		}
	}

	public override void OnError(Exception error)
	{
		if (error == null)
		{
			throw new ArgumentNullException("error");
		}
		IObserver<T>[] array = null;
		lock (_gate)
		{
			CheckDisposed();
			if (!_isStopped)
			{
				array = _observers.Data;
				_observers = ImmutableList<IObserver<T>>.Empty;
				_isStopped = true;
				_exception = error;
			}
		}
		if (array != null)
		{
			IObserver<T>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnError(error);
			}
		}
	}

	public override void OnNext(T value)
	{
		IObserver<T>[] array = null;
		lock (_gate)
		{
			CheckDisposed();
			if (!_isStopped)
			{
				_value = value;
				array = _observers.Data;
			}
		}
		if (array != null)
		{
			IObserver<T>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnNext(value);
			}
		}
	}

	public override IDisposable Subscribe(IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		Exception exception;
		lock (_gate)
		{
			CheckDisposed();
			if (!_isStopped)
			{
				_observers = _observers.Add(observer);
				observer.OnNext(_value);
				return new Subscription(this, observer);
			}
			exception = _exception;
		}
		if (exception != null)
		{
			observer.OnError(exception);
		}
		else
		{
			observer.OnCompleted();
		}
		return Disposable.Empty;
	}

	private void Unsubscribe(IObserver<T> observer)
	{
		lock (_gate)
		{
			if (!_isDisposed)
			{
				_observers = _observers.Remove(observer);
			}
		}
	}

	public override void Dispose()
	{
		lock (_gate)
		{
			_isDisposed = true;
			_observers = null;
			_value = default(T);
			_exception = null;
		}
	}

	private void CheckDisposed()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException(string.Empty);
		}
	}
}

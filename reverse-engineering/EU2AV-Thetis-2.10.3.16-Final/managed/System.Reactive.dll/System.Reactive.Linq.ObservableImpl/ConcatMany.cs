using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ConcatMany<T> : IObservable<T>
{
	internal sealed class ConcatManyOuterObserver : IObserver<IObservable<T>>, IDisposable
	{
		internal sealed class InnerObserver : IObserver<T>, IDisposable
		{
			private readonly ConcatManyOuterObserver _parent;

			internal IDisposable? Upstream;

			internal InnerObserver(ConcatManyOuterObserver parent)
			{
				_parent = parent;
			}

			internal bool SetDisposable(SingleAssignmentDisposable sad)
			{
				return Disposable.TrySetSingle(ref Upstream, sad) == TrySetSingleResult.Success;
			}

			internal bool Finish()
			{
				IDisposable disposable = Volatile.Read(ref Upstream);
				if (disposable != BooleanDisposable.True && Interlocked.CompareExchange(ref Upstream, null, disposable) == disposable)
				{
					disposable.Dispose();
					return true;
				}
				return false;
			}

			public void Dispose()
			{
				Disposable.Dispose(ref Upstream);
			}

			public void OnCompleted()
			{
				_parent.InnerComplete();
			}

			public void OnError(Exception error)
			{
				_parent.InnerError(error);
			}

			public void OnNext(T value)
			{
				_parent.InnerNext(value);
			}
		}

		private readonly IObserver<T> _downstream;

		private readonly ConcurrentQueue<IObservable<T>> _queue;

		private readonly InnerObserver _innerObserver;

		private SingleAssignmentDisposableValue _upstream;

		private int _trampoline;

		private Exception? _error;

		private bool _done;

		private int _active;

		internal ConcatManyOuterObserver(IObserver<T> downstream)
		{
			_downstream = downstream;
			_queue = new ConcurrentQueue<IObservable<T>>();
			_innerObserver = new InnerObserver(this);
		}

		internal void OnSubscribe(IDisposable d)
		{
			_upstream.Disposable = d;
		}

		public void Dispose()
		{
			_innerObserver.Dispose();
			DisposeMain();
		}

		private void DisposeMain()
		{
			_upstream.Dispose();
		}

		private bool IsDisposed()
		{
			return _upstream.IsDisposed;
		}

		public void OnCompleted()
		{
			Volatile.Write(ref _done, value: true);
			Drain();
		}

		public void OnError(Exception error)
		{
			if (Interlocked.CompareExchange(ref _error, error, null) == null)
			{
				Volatile.Write(ref _done, value: true);
				Drain();
			}
		}

		public void OnNext(IObservable<T> value)
		{
			_queue.Enqueue(value);
			Drain();
		}

		private void InnerNext(T item)
		{
			_downstream.OnNext(item);
		}

		private void InnerError(Exception error)
		{
			if (_innerObserver.Finish() && Interlocked.CompareExchange(ref _error, error, null) == null)
			{
				Volatile.Write(ref _done, value: true);
				Volatile.Write(ref _active, 0);
				Drain();
			}
		}

		private void InnerComplete()
		{
			if (_innerObserver.Finish())
			{
				Volatile.Write(ref _active, 0);
				Drain();
			}
		}

		private void Drain()
		{
			if (Interlocked.Increment(ref _trampoline) != 1)
			{
				return;
			}
			do
			{
				if (IsDisposed())
				{
					IObservable<T> result;
					while (_queue.TryDequeue(out result))
					{
					}
				}
				else
				{
					if (Volatile.Read(ref _active) != 0)
					{
						continue;
					}
					bool flag = Volatile.Read(ref _done);
					if (flag)
					{
						Exception ex = Volatile.Read(ref _error);
						if (ex != null)
						{
							_downstream.OnError(ex);
							DisposeMain();
							continue;
						}
					}
					if (_queue.TryDequeue(out IObservable<T> result2))
					{
						SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
						if (_innerObserver.SetDisposable(singleAssignmentDisposable))
						{
							Interlocked.Exchange(ref _active, 1);
							singleAssignmentDisposable.Disposable = result2.SubscribeSafe(_innerObserver);
						}
					}
					else if (flag)
					{
						_downstream.OnCompleted();
						DisposeMain();
					}
				}
			}
			while (Interlocked.Decrement(ref _trampoline) != 0);
		}
	}

	private readonly IObservable<IObservable<T>> _sources;

	internal ConcatMany(IObservable<IObservable<T>> sources)
	{
		_sources = sources;
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		ConcatManyOuterObserver concatManyOuterObserver = new ConcatManyOuterObserver(observer);
		IDisposable d = _sources.SubscribeSafe(concatManyOuterObserver);
		concatManyOuterObserver.OnSubscribe(d);
		return concatManyOuterObserver;
	}
}

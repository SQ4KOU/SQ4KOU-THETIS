using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class GetEnumerator<TSource> : IEnumerator<TSource>, IDisposable, IEnumerator, IObserver<TSource>
{
	private readonly ConcurrentQueue<TSource> _queue;

	private TSource? _current;

	private Exception? _error;

	private bool _done;

	private bool _disposed;

	private SingleAssignmentDisposableValue _subscription;

	private readonly SemaphoreSlim _gate;

	public TSource Current => _current;

	object IEnumerator.Current => _current;

	public GetEnumerator()
	{
		_queue = new ConcurrentQueue<TSource>();
		_gate = new SemaphoreSlim(0);
	}

	public IEnumerator<TSource> Run(IObservable<TSource> source)
	{
		_subscription.Disposable = source.Subscribe(this);
		return this;
	}

	public void OnNext(TSource value)
	{
		_queue.Enqueue(value);
		_gate.Release();
	}

	public void OnError(Exception error)
	{
		_error = error;
		_subscription.Dispose();
		_gate.Release();
	}

	public void OnCompleted()
	{
		_done = true;
		_subscription.Dispose();
		_gate.Release();
	}

	public bool MoveNext()
	{
		_gate.Wait();
		if (_disposed)
		{
			throw new ObjectDisposedException("");
		}
		if (_queue.TryDequeue(out _current))
		{
			return true;
		}
		_error?.Throw();
		_gate.Release();
		return false;
	}

	public void Dispose()
	{
		_subscription.Dispose();
		_disposed = true;
		_gate.Release();
	}

	public void Reset()
	{
		throw new NotSupportedException();
	}
}

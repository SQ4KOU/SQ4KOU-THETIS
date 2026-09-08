using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal abstract class PushToPullSink<TSource, TResult> : IObserver<TSource>, IEnumerator<TResult>, IDisposable, IEnumerator
{
	private SingleAssignmentDisposableValue _upstream;

	private bool _done;

	public TResult Current { get; private set; }

	object IEnumerator.Current => Current;

	public abstract void OnNext(TSource value);

	public abstract void OnError(Exception error);

	public abstract void OnCompleted();

	public abstract bool TryMoveNext([MaybeNullWhen(false)] out TResult current);

	public bool MoveNext()
	{
		if (!_done)
		{
			if (TryMoveNext(out var current))
			{
				Current = current;
				return true;
			}
			_done = true;
			Dispose();
		}
		return false;
	}

	public void Reset()
	{
		throw new NotSupportedException();
	}

	public void Dispose()
	{
		_upstream.Dispose();
	}

	public void SetUpstream(IDisposable d)
	{
		_upstream.Disposable = d;
	}
}

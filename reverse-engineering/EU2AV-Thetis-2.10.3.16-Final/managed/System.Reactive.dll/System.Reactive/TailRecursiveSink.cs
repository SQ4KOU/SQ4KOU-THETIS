using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive;

internal abstract class TailRecursiveSink<TSource>(IObserver<TSource> observer) : IdentitySink<TSource>(observer)
{
	private readonly Stack<IEnumerator<IObservable<TSource>>> _stack = new Stack<IEnumerator<IObservable<TSource>>>();

	private bool _isDisposed;

	private int _trampoline;

	private IDisposable? _currentSubscription;

	public void Run(IEnumerable<IObservable<TSource>> sources)
	{
		if (TryGetEnumerator(sources, out IEnumerator<IObservable<TSource>> result))
		{
			_stack.Push(result);
			Drain();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			DisposeAll();
		}
		base.Dispose(disposing);
	}

	private void Drain()
	{
		if (Interlocked.Increment(ref _trampoline) != 1)
		{
			return;
		}
		do
		{
			IL_000f:
			if (Volatile.Read(ref _isDisposed))
			{
				while (_stack.Count != 0)
				{
					_stack.Pop().Dispose();
				}
				Disposable.Dispose(ref _currentSubscription);
			}
			else if (_stack.Count != 0)
			{
				IEnumerator<IObservable<TSource>> enumerator = _stack.Peek();
				IObservable<TSource> source = null;
				try
				{
					if (enumerator.MoveNext())
					{
						source = enumerator.Current;
					}
				}
				catch (Exception error)
				{
					enumerator.Dispose();
					ForwardOnError(error);
					Volatile.Write(ref _isDisposed, value: true);
					goto IL_000f;
				}
				IObservable<TSource> observable;
				try
				{
					observable = Unpack<TSource>(source);
				}
				catch (Exception error2)
				{
					if (!Fail(error2))
					{
						Volatile.Write(ref _isDisposed, value: true);
					}
					goto IL_000f;
				}
				if (observable == null)
				{
					_stack.Pop();
					enumerator.Dispose();
					goto IL_000f;
				}
				IEnumerable<IObservable<TSource>> enumerable = Extract(observable);
				if (enumerable != null)
				{
					if (TryGetEnumerator(enumerable, out IEnumerator<IObservable<TSource>> result))
					{
						_stack.Push(result);
					}
					else
					{
						Volatile.Write(ref _isDisposed, value: true);
					}
					goto IL_000f;
				}
				IDisposable ready = ReadyToken.Ready;
				if (Disposable.TrySetSingle(ref _currentSubscription, ready) != TrySetSingleResult.Success)
				{
					goto IL_000f;
				}
				IDisposable disposable = observable.SubscribeSafe(this);
				IDisposable disposable2 = Interlocked.CompareExchange(ref _currentSubscription, disposable, ready);
				if (disposable2 != ready)
				{
					disposable.Dispose();
					if (disposable2 == BooleanDisposable.True)
					{
						goto IL_000f;
					}
				}
			}
			else
			{
				Volatile.Write(ref _isDisposed, value: true);
				Done();
			}
		}
		while (Interlocked.Decrement(ref _trampoline) != 0);
		static IObservable<T>? Unpack<T>(IObservable<T>? observable2) where T : notnull
		{
			bool flag;
			do
			{
				flag = false;
				if (observable2 is IEvaluatableObservable<T> evaluatableObservable)
				{
					observable2 = evaluatableObservable.Eval();
					flag = true;
				}
			}
			while (flag);
			return observable2;
		}
	}

	private void DisposeAll()
	{
		Volatile.Write(ref _isDisposed, value: true);
		Drain();
	}

	protected void Recurse()
	{
		if (Disposable.TrySetSerial(ref _currentSubscription, null))
		{
			Drain();
		}
	}

	protected abstract IEnumerable<IObservable<TSource>>? Extract(IObservable<TSource> source);

	private bool TryGetEnumerator(IEnumerable<IObservable<TSource>> sources, [NotNullWhen(true)] out IEnumerator<IObservable<TSource>>? result)
	{
		try
		{
			result = sources.GetEnumerator();
			return true;
		}
		catch (Exception error)
		{
			ForwardOnError(error);
			result = null;
			return false;
		}
	}

	protected virtual void Done()
	{
		ForwardOnCompleted();
	}

	protected virtual bool Fail(Exception error)
	{
		ForwardOnError(error);
		return false;
	}
}

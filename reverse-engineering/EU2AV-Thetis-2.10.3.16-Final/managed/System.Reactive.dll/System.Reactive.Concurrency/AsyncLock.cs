using System.Collections.Generic;

namespace System.Reactive.Concurrency;

public sealed class AsyncLock : IDisposable
{
	private bool _isAcquired;

	private bool _hasFaulted;

	private readonly object _guard = new object();

	private Queue<(Action<Delegate, object?> action, Delegate @delegate, object? state)>? _queue;

	public void Wait(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		Wait(action, delegate(Action closureAction)
		{
			closureAction();
		});
	}

	internal void Wait<TState>(TState state, Action<TState> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		Wait(state, action, delegate(Delegate actionObject, object? stateObject)
		{
			((Action<TState>)actionObject)((TState)stateObject);
		});
	}

	private void Wait(object? state, Delegate @delegate, Action<Delegate, object?> action)
	{
		lock (_guard)
		{
			if (_hasFaulted)
			{
				return;
			}
			if (_isAcquired)
			{
				Queue<(Action<Delegate, object>, Delegate, object)> queue = _queue;
				if (queue == null)
				{
					queue = (_queue = new Queue<(Action<Delegate, object>, Delegate, object)>());
				}
				queue.Enqueue((action, @delegate, state));
				return;
			}
			_isAcquired = true;
		}
		while (true)
		{
			try
			{
				action(@delegate, state);
			}
			catch
			{
				lock (_guard)
				{
					_queue = null;
					_hasFaulted = true;
				}
				throw;
			}
			lock (_guard)
			{
				Queue<(Action<Delegate, object>, Delegate, object)> queue2 = _queue;
				if (queue2 == null || queue2.Count == 0)
				{
					_isAcquired = false;
					break;
				}
				(action, @delegate, state) = queue2.Dequeue();
			}
		}
	}

	public void Dispose()
	{
		lock (_guard)
		{
			_queue = null;
			_hasFaulted = true;
		}
	}
}

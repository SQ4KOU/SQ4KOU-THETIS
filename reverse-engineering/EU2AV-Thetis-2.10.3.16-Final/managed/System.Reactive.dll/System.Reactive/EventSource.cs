using System.Collections.Generic;

namespace System.Reactive;

internal sealed class EventSource<T> : IEventSource<T>
{
	private readonly IObservable<T> _source;

	private readonly Dictionary<Delegate, Stack<IDisposable>> _subscriptions = new Dictionary<Delegate, Stack<IDisposable>>();

	private readonly Action<Action<T>, T> _invokeHandler;

	public event Action<T> OnNext
	{
		add
		{
			object gate = new object();
			bool isAdded = false;
			bool isDone = false;
			Action remove = delegate
			{
				lock (gate)
				{
					if (isAdded)
					{
						Remove(value);
					}
					else
					{
						isDone = true;
					}
				}
			};
			IDisposable disposable = _source.Subscribe(delegate(T x)
			{
				_invokeHandler(value, x);
			}, delegate(Exception ex)
			{
				remove();
				ex.Throw();
			}, remove);
			lock (gate)
			{
				if (!isDone)
				{
					Add(value, disposable);
					isAdded = true;
				}
			}
		}
		remove
		{
			Remove(value);
		}
	}

	public EventSource(IObservable<T> source, Action<Action<T>, T> invokeHandler)
	{
		_source = source;
		_invokeHandler = invokeHandler;
	}

	private void Add(Delegate handler, IDisposable disposable)
	{
		lock (_subscriptions)
		{
			if (!_subscriptions.TryGetValue(handler, out Stack<IDisposable> value))
			{
				value = (_subscriptions[handler] = new Stack<IDisposable>());
			}
			value.Push(disposable);
		}
	}

	private void Remove(Delegate handler)
	{
		IDisposable disposable = null;
		lock (_subscriptions)
		{
			Stack<IDisposable> value = new Stack<IDisposable>();
			if (_subscriptions.TryGetValue(handler, out value))
			{
				disposable = value.Pop();
				if (value.Count == 0)
				{
					_subscriptions.Remove(handler);
				}
			}
		}
		disposable?.Dispose();
	}
}

using System.Collections.Generic;

namespace System.Reactive;

public abstract class EventPatternSourceBase<TSender, TEventArgs>
{
	private sealed class Observer : ObserverBase<EventPattern<TSender, TEventArgs>>, ISafeObserver<EventPattern<TSender, TEventArgs>>, IObserver<EventPattern<TSender, TEventArgs>>, IDisposable
	{
		private bool _isDone;

		private bool _isAdded;

		private readonly Delegate _handler;

		private readonly object _gate = new object();

		private readonly Action<TSender?, TEventArgs> _invoke;

		private readonly EventPatternSourceBase<TSender, TEventArgs> _sourceBase;

		public Observer(EventPatternSourceBase<TSender, TEventArgs> sourceBase, Delegate handler, Action<TSender?, TEventArgs> invoke)
		{
			_handler = handler;
			_invoke = invoke;
			_sourceBase = sourceBase;
		}

		protected override void OnNextCore(EventPattern<TSender, TEventArgs> value)
		{
			_sourceBase._invokeHandler(_invoke, value);
		}

		protected override void OnErrorCore(Exception error)
		{
			Remove();
			error.Throw();
		}

		protected override void OnCompletedCore()
		{
			Remove();
		}

		private void Remove()
		{
			lock (_gate)
			{
				if (_isAdded)
				{
					_sourceBase.Remove(_handler);
				}
				else
				{
					_isDone = true;
				}
			}
		}

		public void SetResource(IDisposable resource)
		{
			lock (_gate)
			{
				if (!_isDone)
				{
					_sourceBase.Add(_handler, resource);
					_isAdded = true;
				}
			}
		}
	}

	private readonly IObservable<EventPattern<TSender, TEventArgs>> _source;

	private readonly Dictionary<Delegate, Stack<IDisposable>> _subscriptions = new Dictionary<Delegate, Stack<IDisposable>>();

	private readonly Action<Action<TSender?, TEventArgs>, EventPattern<TSender, TEventArgs>> _invokeHandler;

	protected EventPatternSourceBase(IObservable<EventPattern<TSender, TEventArgs>> source, Action<Action<TSender?, TEventArgs>, EventPattern<TSender, TEventArgs>> invokeHandler)
	{
		_source = source ?? throw new ArgumentNullException("source");
		_invokeHandler = invokeHandler ?? throw new ArgumentNullException("invokeHandler");
	}

	protected void Add(Delegate handler, Action<TSender?, TEventArgs> invoke)
	{
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (invoke == null)
		{
			throw new ArgumentNullException("invoke");
		}
		Observer observer = new Observer(this, handler, invoke);
		observer.SetResource(_source.Subscribe(observer));
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

	protected void Remove(Delegate handler)
	{
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		IDisposable disposable = null;
		lock (_subscriptions)
		{
			if (_subscriptions.TryGetValue(handler, out Stack<IDisposable> value))
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

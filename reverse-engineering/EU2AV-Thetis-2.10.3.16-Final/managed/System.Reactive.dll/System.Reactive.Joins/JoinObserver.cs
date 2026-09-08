using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace System.Reactive.Joins;

internal sealed class JoinObserver<T> : ObserverBase<Notification<T>>, IJoinObserver, IDisposable
{
	private object? _gate;

	private readonly IObservable<T> _source;

	private readonly Action<Exception> _onError;

	private readonly List<ActivePlan> _activePlans = new List<ActivePlan>();

	private SingleAssignmentDisposableValue _subscription;

	private bool _isDisposed;

	public Queue<Notification<T>> Queue { get; }

	public JoinObserver(IObservable<T> source, Action<Exception> onError)
	{
		_source = source;
		_onError = onError;
		Queue = new Queue<Notification<T>>();
	}

	public void AddActivePlan(ActivePlan activePlan)
	{
		_activePlans.Add(activePlan);
	}

	public void Subscribe(object gate)
	{
		_gate = gate;
		_subscription.Disposable = _source.Materialize().SubscribeSafe(this);
	}

	public void Dequeue()
	{
		Queue.Dequeue();
	}

	protected override void OnNextCore(Notification<T> notification)
	{
		lock (_gate)
		{
			if (_isDisposed)
			{
				return;
			}
			if (notification.Kind == NotificationKind.OnError)
			{
				_onError(notification.Exception);
				return;
			}
			Queue.Enqueue(notification);
			ActivePlan[] array = _activePlans.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Match();
			}
		}
	}

	protected override void OnErrorCore(Exception exception)
	{
	}

	protected override void OnCompletedCore()
	{
	}

	internal void RemoveActivePlan(ActivePlan activePlan)
	{
		_activePlans.Remove(activePlan);
		if (_activePlans.Count == 0)
		{
			Dispose();
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!_isDisposed)
		{
			if (disposing)
			{
				_subscription.Dispose();
			}
			_isDisposed = true;
		}
	}
}

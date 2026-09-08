using System.Reactive.Linq;

namespace System.Reactive.Subjects;

internal sealed class ConnectableObservable<TSource, TResult> : IConnectableObservable<TResult>, IObservable<TResult>
{
	private sealed class Connection : IDisposable
	{
		private readonly ConnectableObservable<TSource, TResult> _parent;

		private IDisposable? _subscription;

		public Connection(ConnectableObservable<TSource, TResult> parent, IDisposable subscription)
		{
			_parent = parent;
			_subscription = subscription;
		}

		public void Dispose()
		{
			lock (_parent._gate)
			{
				if (_subscription != null)
				{
					_subscription.Dispose();
					_subscription = null;
					_parent._connection = null;
				}
			}
		}
	}

	private readonly ISubject<TSource, TResult> _subject;

	private readonly IObservable<TSource> _source;

	private readonly object _gate;

	private Connection? _connection;

	public ConnectableObservable(IObservable<TSource> source, ISubject<TSource, TResult> subject)
	{
		_subject = subject;
		_source = source.AsObservable();
		_gate = new object();
	}

	public IDisposable Connect()
	{
		lock (_gate)
		{
			if (_connection == null)
			{
				IDisposable subscription = _source.SubscribeSafe(_subject);
				_connection = new Connection(this, subscription);
			}
			return _connection;
		}
	}

	public IDisposable Subscribe(IObserver<TResult> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return _subject.SubscribeSafe(observer);
	}
}

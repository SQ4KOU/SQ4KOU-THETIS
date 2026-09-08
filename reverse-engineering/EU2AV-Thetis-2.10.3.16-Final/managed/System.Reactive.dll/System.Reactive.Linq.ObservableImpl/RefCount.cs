using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal static class RefCount<TSource>
{
	internal sealed class Eager : Producer<TSource, Eager._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly Eager _parent;

			private RefConnection? _targetConnection;

			public _(IObserver<TSource> observer, Eager parent)
				: base(observer)
			{
				_parent = parent;
			}

			public void Run()
			{
				RefConnection refConnection;
				bool flag;
				lock (_parent._gate)
				{
					refConnection = _parent._connection;
					if (refConnection == null)
					{
						refConnection = new RefConnection();
						_parent._connection = refConnection;
					}
					flag = ++refConnection._count == _parent._minObservers && refConnection._disposable.Disposable == null;
					_targetConnection = refConnection;
				}
				Run(_parent._source);
				if (flag && !refConnection._disposable.IsDisposed)
				{
					refConnection._disposable.Disposable = _parent._source.Connect();
				}
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (!disposing)
				{
					return;
				}
				RefConnection targetConnection = _targetConnection;
				_targetConnection = null;
				lock (_parent._gate)
				{
					if (targetConnection != _parent._connection || --targetConnection._count != 0)
					{
						return;
					}
					_parent._connection = null;
				}
				targetConnection._disposable.Dispose();
			}
		}

		private sealed class RefConnection
		{
			internal int _count;

			internal SingleAssignmentDisposableValue _disposable;
		}

		private readonly IConnectableObservable<TSource> _source;

		private readonly object _gate = new object();

		private RefConnection? _connection;

		private readonly int _minObservers;

		public Eager(IConnectableObservable<TSource> source, int minObservers)
		{
			_source = source;
			_minObservers = minObservers;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(observer, this);
		}

		protected override void Run(@_ sink)
		{
			sink.Run();
		}
	}

	internal sealed class Lazy : Producer<TSource, Lazy._>
	{
		private enum State
		{
			DisconnectedNoSubscribers,
			DisconnectedWithSubscribers,
			ConnectedWithSubscribers,
			ConnectedWithNoSubscribers
		}

		internal sealed class @_ : IdentitySink<TSource>
		{
			public _(IObserver<TSource> observer)
				: base(observer)
			{
			}

			public void Run(Lazy parent)
			{
				IDisposable item = parent._source.SubscribeSafe(this);
				lock (parent._gate)
				{
					int num = ++parent._count;
					bool flag = false;
					bool flag2 = false;
					switch (parent._state)
					{
					case State.DisconnectedNoSubscribers:
					case State.DisconnectedWithSubscribers:
						flag = num == parent._minObservers;
						parent._state = ((!flag) ? State.DisconnectedWithSubscribers : State.ConnectedWithSubscribers);
						break;
					case State.ConnectedWithNoSubscribers:
						flag2 = true;
						parent._state = State.ConnectedWithSubscribers;
						break;
					}
					if (flag)
					{
						parent._connectableSubscription = parent._source.Connect();
					}
					if (flag | flag2)
					{
						Disposable.TrySetSerial(ref parent._serial, new SingleAssignmentDisposable());
					}
				}
				SetUpstream(Disposable.Create((parent, item), delegate((Lazy parent, IDisposable subscription) tuple)
				{
					(Lazy parent, IDisposable subscription) tuple2 = tuple;
					var (lazy, _) = tuple2;
					tuple2.subscription.Dispose();
					lock (lazy._gate)
					{
						if (--lazy._count == 0)
						{
							if (lazy._state == State.ConnectedWithSubscribers)
							{
								lazy._state = State.ConnectedWithNoSubscribers;
								SingleAssignmentDisposable singleAssignmentDisposable = (SingleAssignmentDisposable)Volatile.Read(ref lazy._serial);
								singleAssignmentDisposable.Disposable = lazy._scheduler.ScheduleAction((singleAssignmentDisposable, lazy), lazy._disconnectTime, delegate((SingleAssignmentDisposable cancelable, Lazy closureParent) tuple4)
								{
									lock (tuple4.closureParent._gate)
									{
										if (Volatile.Read(ref tuple4.closureParent._serial) == tuple4.cancelable)
										{
											tuple4.closureParent._state = State.DisconnectedNoSubscribers;
											IDisposable? connectableSubscription = tuple4.closureParent._connectableSubscription;
											tuple4.closureParent._connectableSubscription = null;
											connectableSubscription.Dispose();
										}
									}
								});
							}
							else
							{
								lazy._state = State.DisconnectedNoSubscribers;
							}
						}
					}
				}));
			}
		}

		private readonly object _gate;

		private readonly IScheduler _scheduler;

		private readonly TimeSpan _disconnectTime;

		private readonly IConnectableObservable<TSource> _source;

		private readonly int _minObservers;

		private State _state;

		private IDisposable? _serial;

		private int _count;

		private IDisposable? _connectableSubscription;

		public Lazy(IConnectableObservable<TSource> source, TimeSpan disconnectTime, IScheduler scheduler, int minObservers)
		{
			_source = source;
			_gate = new object();
			_disconnectTime = disconnectTime;
			_scheduler = scheduler;
			_minObservers = minObservers;
			_state = State.DisconnectedNoSubscribers;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}
}

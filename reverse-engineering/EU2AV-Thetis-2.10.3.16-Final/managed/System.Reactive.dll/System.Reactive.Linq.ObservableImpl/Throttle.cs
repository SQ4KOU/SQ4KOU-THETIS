using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Throttle<TSource> : Producer<TSource, Throttle<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly object _gate = new object();

		private readonly TimeSpan _dueTime;

		private readonly IScheduler _scheduler;

		private TSource? _value;

		private bool _hasValue;

		private SerialDisposableValue _serialCancelable;

		private ulong _id;

		public _(Throttle<TSource> parent, IObserver<TSource> observer)
			: base(observer)
		{
			_dueTime = parent._dueTime;
			_scheduler = parent._scheduler;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_serialCancelable.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnNext(TSource value)
		{
			ulong id;
			lock (_gate)
			{
				_hasValue = true;
				_value = value;
				_id++;
				id = _id;
			}
			_serialCancelable.Disposable = null;
			_serialCancelable.Disposable = _scheduler.ScheduleAction((this, id), _dueTime, delegate((@_ @this, ulong currentid) tuple)
			{
				tuple.@this.Propagate(tuple.currentid);
			});
		}

		private void Propagate(ulong currentid)
		{
			lock (_gate)
			{
				if (_hasValue && _id == currentid)
				{
					ForwardOnNext(_value);
					_hasValue = false;
				}
			}
		}

		public override void OnError(Exception error)
		{
			_serialCancelable.Dispose();
			lock (_gate)
			{
				ForwardOnError(error);
				_hasValue = false;
				_id++;
			}
		}

		public override void OnCompleted()
		{
			_serialCancelable.Dispose();
			lock (_gate)
			{
				if (_hasValue)
				{
					ForwardOnNext(_value);
				}
				ForwardOnCompleted();
				_hasValue = false;
				_id++;
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly TimeSpan _dueTime;

	private readonly IScheduler _scheduler;

	public Throttle(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		_source = source;
		_dueTime = dueTime;
		_scheduler = scheduler;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
internal sealed class Throttle<TSource, TThrottle> : Producer<TSource, Throttle<TSource, TThrottle>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private sealed class ThrottleObserver : SafeObserver<TThrottle>
		{
			private readonly @_ _parent;

			private readonly TSource _value;

			private readonly ulong _currentid;

			public ThrottleObserver(@_ parent, TSource value, ulong currentid)
			{
				_parent = parent;
				_value = value;
				_currentid = currentid;
			}

			public override void OnNext(TThrottle value)
			{
				lock (_parent._gate)
				{
					if (_parent._hasValue && _parent._id == _currentid)
					{
						_parent.ForwardOnNext(_value);
					}
					_parent._hasValue = false;
					Dispose();
				}
			}

			public override void OnError(Exception error)
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_parent._gate)
				{
					if (_parent._hasValue && _parent._id == _currentid)
					{
						_parent.ForwardOnNext(_value);
					}
					_parent._hasValue = false;
					Dispose();
				}
			}
		}

		private readonly object _gate = new object();

		private readonly Func<TSource, IObservable<TThrottle>> _throttleSelector;

		private TSource? _value;

		private bool _hasValue;

		private SerialDisposableValue _serialCancelable;

		private ulong _id;

		public _(Throttle<TSource, TThrottle> parent, IObserver<TSource> observer)
			: base(observer)
		{
			_throttleSelector = parent._throttleSelector;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_serialCancelable.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnNext(TSource value)
		{
			IObservable<TThrottle> source;
			try
			{
				source = _throttleSelector(value);
			}
			catch (Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
					return;
				}
			}
			ulong id;
			lock (_gate)
			{
				_hasValue = true;
				_value = value;
				_id++;
				id = _id;
			}
			_serialCancelable.Disposable = null;
			ThrottleObserver throttleObserver = new ThrottleObserver(this, value, id);
			throttleObserver.SetResource(source.SubscribeSafe(throttleObserver));
			_serialCancelable.Disposable = throttleObserver;
		}

		public override void OnError(Exception error)
		{
			_serialCancelable.Dispose();
			lock (_gate)
			{
				ForwardOnError(error);
				_hasValue = false;
				_id++;
			}
		}

		public override void OnCompleted()
		{
			_serialCancelable.Dispose();
			lock (_gate)
			{
				if (_hasValue)
				{
					ForwardOnNext(_value);
				}
				ForwardOnCompleted();
				_hasValue = false;
				_id++;
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, IObservable<TThrottle>> _throttleSelector;

	public Throttle(IObservable<TSource> source, Func<TSource, IObservable<TThrottle>> throttleSelector)
	{
		_source = source;
		_throttleSelector = throttleSelector;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

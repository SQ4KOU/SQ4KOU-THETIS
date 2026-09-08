using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Timeout<TSource>
{
	internal sealed class Relative : Producer<TSource, Relative._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly TimeSpan _dueTime;

			private readonly IObservable<TSource> _other;

			private readonly IScheduler _scheduler;

			private long _index;

			private SingleAssignmentDisposableValue _mainDisposable;

			private SingleAssignmentDisposableValue _otherDisposable;

			private IDisposable? _timerDisposable;

			public _(Relative parent, IObserver<TSource> observer)
				: base(observer)
			{
				_dueTime = parent._dueTime;
				_other = parent._other;
				_scheduler = parent._scheduler;
			}

			public override void Run(IObservable<TSource> source)
			{
				CreateTimer(0L);
				_mainDisposable.Disposable = source.SubscribeSafe(this);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_mainDisposable.Dispose();
					_otherDisposable.Dispose();
					Disposable.Dispose(ref _timerDisposable);
				}
				base.Dispose(disposing);
			}

			private void CreateTimer(long idx)
			{
				if (Disposable.TrySetMultiple(ref _timerDisposable, null))
				{
					IDisposable value = _scheduler.ScheduleAction((idx, this), _dueTime, delegate((long idx, @_ instance) state)
					{
						state.instance.Timeout(state.idx);
					});
					Disposable.TrySetMultiple(ref _timerDisposable, value);
				}
			}

			private void Timeout(long idx)
			{
				if (Volatile.Read(ref _index) == idx && Interlocked.CompareExchange(ref _index, long.MaxValue, idx) == idx)
				{
					_mainDisposable.Dispose();
					IDisposable disposable = _other.Subscribe(GetForwarder());
					_otherDisposable.Disposable = disposable;
				}
			}

			public override void OnNext(TSource value)
			{
				long num = Volatile.Read(ref _index);
				if (num != long.MaxValue && Interlocked.CompareExchange(ref _index, num + 1, num) == num)
				{
					Volatile.Read(ref _timerDisposable)?.Dispose();
					ForwardOnNext(value);
					CreateTimer(num + 1);
				}
			}

			public override void OnError(Exception error)
			{
				if (Interlocked.Exchange(ref _index, long.MaxValue) != long.MaxValue)
				{
					Disposable.Dispose(ref _timerDisposable);
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (Interlocked.Exchange(ref _index, long.MaxValue) != long.MaxValue)
				{
					Disposable.Dispose(ref _timerDisposable);
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly TimeSpan _dueTime;

		private readonly IObservable<TSource> _other;

		private readonly IScheduler _scheduler;

		public Relative(IObservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other, IScheduler scheduler)
		{
			_source = source;
			_dueTime = dueTime;
			_other = other;
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

	internal sealed class Absolute : Producer<TSource, Absolute._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly IObservable<TSource> _other;

			private SerialDisposableValue _serialDisposable;

			private int _wip;

			public _(IObservable<TSource> other, IObserver<TSource> observer)
				: base(observer)
			{
				_other = other;
			}

			public void Run(Absolute parent)
			{
				SetUpstream(parent._scheduler.ScheduleAction(this, parent._dueTime, delegate(@_ @this)
				{
					@this.Timeout();
				}));
				_serialDisposable.Disposable = parent._source.SubscribeSafe(this);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_serialDisposable.Dispose();
				}
				base.Dispose(disposing);
			}

			private void Timeout()
			{
				if (Interlocked.Increment(ref _wip) == 1)
				{
					_serialDisposable.Disposable = _other.SubscribeSafe(GetForwarder());
				}
			}

			public override void OnNext(TSource value)
			{
				if (Interlocked.CompareExchange(ref _wip, 1, 0) == 0)
				{
					ForwardOnNext(value);
					if (Interlocked.Decrement(ref _wip) != 0)
					{
						_serialDisposable.Disposable = _other.SubscribeSafe(GetForwarder());
					}
				}
			}

			public override void OnError(Exception error)
			{
				if (Interlocked.CompareExchange(ref _wip, 1, 0) == 0)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (Interlocked.CompareExchange(ref _wip, 1, 0) == 0)
				{
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly DateTimeOffset _dueTime;

		private readonly IObservable<TSource> _other;

		private readonly IScheduler _scheduler;

		public Absolute(IObservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other, IScheduler scheduler)
		{
			_source = source;
			_dueTime = dueTime;
			_other = other;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_other, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}
}
internal sealed class Timeout<TSource, TTimeout> : Producer<TSource, Timeout<TSource, TTimeout>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private sealed class TimeoutObserver : SafeObserver<TTimeout>
		{
			private readonly @_ _parent;

			private readonly long _id;

			public TimeoutObserver(@_ parent, long id)
			{
				_parent = parent;
				_id = id;
			}

			public override void OnNext(TTimeout value)
			{
				OnCompleted();
			}

			public override void OnError(Exception error)
			{
				if (!_parent.TimeoutError(_id, error))
				{
					Dispose();
				}
			}

			public override void OnCompleted()
			{
				_parent.Timeout(_id);
				Dispose();
			}
		}

		private readonly Func<TSource, IObservable<TTimeout>> _timeoutSelector;

		private readonly IObservable<TSource> _other;

		private SerialDisposableValue _sourceDisposable;

		private IDisposable? _timerDisposable;

		private long _index;

		public _(Timeout<TSource, TTimeout> parent, IObserver<TSource> observer)
			: base(observer)
		{
			_timeoutSelector = parent._timeoutSelector;
			_other = parent._other;
		}

		public void Run(Timeout<TSource, TTimeout> parent)
		{
			SetTimer(parent._firstTimeout, 0L);
			_sourceDisposable.TrySetFirst(parent._source.SubscribeSafe(this));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_sourceDisposable.Dispose();
				Disposable.Dispose(ref _timerDisposable);
			}
			base.Dispose(disposing);
		}

		public override void OnNext(TSource value)
		{
			long num = Volatile.Read(ref _index);
			if (num != long.MaxValue && Interlocked.CompareExchange(ref _index, num + 1, num) == num)
			{
				Volatile.Read(ref _timerDisposable)?.Dispose();
				ForwardOnNext(value);
				IObservable<TTimeout> timeout;
				try
				{
					timeout = _timeoutSelector(value);
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				SetTimer(timeout, num + 1);
			}
		}

		public override void OnError(Exception error)
		{
			if (Interlocked.Exchange(ref _index, long.MaxValue) != long.MaxValue)
			{
				ForwardOnError(error);
			}
		}

		public override void OnCompleted()
		{
			if (Interlocked.Exchange(ref _index, long.MaxValue) != long.MaxValue)
			{
				ForwardOnCompleted();
			}
		}

		private void Timeout(long idx)
		{
			if (Volatile.Read(ref _index) == idx && Interlocked.CompareExchange(ref _index, long.MaxValue, idx) == idx)
			{
				_sourceDisposable.Disposable = _other.SubscribeSafe(GetForwarder());
			}
		}

		private bool TimeoutError(long idx, Exception error)
		{
			if (Volatile.Read(ref _index) == idx && Interlocked.CompareExchange(ref _index, long.MaxValue, idx) == idx)
			{
				ForwardOnError(error);
				return true;
			}
			return false;
		}

		private void SetTimer(IObservable<TTimeout> timeout, long idx)
		{
			TimeoutObserver timeoutObserver = new TimeoutObserver(this, idx);
			if (Disposable.TrySetSerial(ref _timerDisposable, timeoutObserver))
			{
				IDisposable resource = timeout.Subscribe(timeoutObserver);
				timeoutObserver.SetResource(resource);
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly IObservable<TTimeout> _firstTimeout;

	private readonly Func<TSource, IObservable<TTimeout>> _timeoutSelector;

	private readonly IObservable<TSource> _other;

	public Timeout(IObservable<TSource> source, IObservable<TTimeout> firstTimeout, Func<TSource, IObservable<TTimeout>> timeoutSelector, IObservable<TSource> other)
	{
		_source = source;
		_firstTimeout = firstTimeout;
		_timeoutSelector = timeoutSelector;
		_other = other;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(this);
	}
}

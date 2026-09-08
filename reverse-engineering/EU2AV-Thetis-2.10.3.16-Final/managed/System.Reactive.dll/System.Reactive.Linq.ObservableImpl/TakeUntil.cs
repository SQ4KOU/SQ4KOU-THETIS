using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class TakeUntil<TSource, TOther> : Producer<TSource, TakeUntil<TSource, TOther>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private sealed class OtherObserver : IObserver<TOther>
		{
			private readonly @_ _parent;

			public OtherObserver(@_ parent)
			{
				_parent = parent;
			}

			public void OnCompleted()
			{
				_parent._otherDisposable.Dispose();
			}

			public void OnError(Exception error)
			{
				HalfSerializer.ForwardOnError(_parent, error, ref _parent._halfSerializer, ref _parent._error);
			}

			public void OnNext(TOther value)
			{
				HalfSerializer.ForwardOnCompleted(_parent, ref _parent._halfSerializer, ref _parent._error);
			}
		}

		private SingleAssignmentDisposableValue _otherDisposable;

		private int _halfSerializer;

		private Exception? _error;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(TakeUntil<TSource, TOther> parent)
		{
			_otherDisposable.Disposable = parent._other.Subscribe(new OtherObserver(this));
			Run(parent._source);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_otherDisposable.IsDisposed)
			{
				_otherDisposable.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnNext(TSource value)
		{
			HalfSerializer.ForwardOnNext(this, value, ref _halfSerializer, ref _error);
		}

		public override void OnError(Exception ex)
		{
			HalfSerializer.ForwardOnError(this, ex, ref _halfSerializer, ref _error);
		}

		public override void OnCompleted()
		{
			HalfSerializer.ForwardOnCompleted(this, ref _halfSerializer, ref _error);
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly IObservable<TOther> _other;

	public TakeUntil(IObservable<TSource> source, IObservable<TOther> other)
	{
		_source = source;
		_other = other;
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
internal sealed class TakeUntil<TSource> : Producer<TSource, TakeUntil<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private SingleAssignmentDisposableValue _timerDisposable;

		private int _wip;

		private Exception? _error;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(TakeUntil<TSource> parent)
		{
			_timerDisposable.Disposable = parent._scheduler.ScheduleAction(this, parent._endTime, delegate(@_ state)
			{
				state.Tick();
			});
			Run(parent._source);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timerDisposable.Dispose();
			}
			base.Dispose(disposing);
		}

		private void Tick()
		{
			OnCompleted();
		}

		public override void OnNext(TSource value)
		{
			HalfSerializer.ForwardOnNext(this, value, ref _wip, ref _error);
		}

		public override void OnError(Exception error)
		{
			HalfSerializer.ForwardOnError(this, error, ref _wip, ref _error);
		}

		public override void OnCompleted()
		{
			HalfSerializer.ForwardOnCompleted(this, ref _wip, ref _error);
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly DateTimeOffset _endTime;

	internal readonly IScheduler _scheduler;

	public TakeUntil(IObservable<TSource> source, DateTimeOffset endTime, IScheduler scheduler)
	{
		_source = source;
		_endTime = endTime;
		_scheduler = scheduler;
	}

	public IObservable<TSource> Combine(DateTimeOffset endTime)
	{
		if (_endTime <= endTime)
		{
			return this;
		}
		return new TakeUntil<TSource>(_source, endTime, _scheduler);
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

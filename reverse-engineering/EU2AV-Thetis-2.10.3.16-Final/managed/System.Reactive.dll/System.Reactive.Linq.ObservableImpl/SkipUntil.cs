using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SkipUntil<TSource, TOther> : Producer<TSource, SkipUntil<TSource, TOther>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private sealed class OtherObserver : IObserver<TOther>, IDisposable
		{
			private readonly @_ _parent;

			public OtherObserver(@_ parent)
			{
				_parent = parent;
			}

			public void Dispose()
			{
				if (!_parent._otherDisposable.IsDisposed)
				{
					_parent._otherDisposable.Dispose();
				}
			}

			public void OnCompleted()
			{
				Dispose();
			}

			public void OnError(Exception error)
			{
				HalfSerializer.ForwardOnError(_parent, error, ref _parent._halfSerializer, ref _parent._error);
			}

			public void OnNext(TOther value)
			{
				_parent.OtherComplete();
				Dispose();
			}
		}

		private SingleAssignmentDisposableValue _otherDisposable;

		private bool _forward;

		private int _halfSerializer;

		private Exception? _error;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(SkipUntil<TSource, TOther> parent)
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
			if (_forward)
			{
				HalfSerializer.ForwardOnNext(this, value, ref _halfSerializer, ref _error);
			}
		}

		public override void OnError(Exception ex)
		{
			HalfSerializer.ForwardOnError(this, ex, ref _halfSerializer, ref _error);
		}

		public override void OnCompleted()
		{
			if (_forward)
			{
				HalfSerializer.ForwardOnCompleted(this, ref _halfSerializer, ref _error);
			}
			else
			{
				DisposeUpstream();
			}
		}

		private void OtherComplete()
		{
			_forward = true;
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly IObservable<TOther> _other;

	public SkipUntil(IObservable<TSource> source, IObservable<TOther> other)
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
internal sealed class SkipUntil<TSource> : Producer<TSource, SkipUntil<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private bool _open;

		private SingleAssignmentDisposableValue _task;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(SkipUntil<TSource> parent)
		{
			_task.Disposable = parent._scheduler.ScheduleAction(this, parent._startTime, delegate(@_ state)
			{
				state.Tick();
			});
			Run(parent._source);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_task.Dispose();
			}
			base.Dispose(disposing);
		}

		private void Tick()
		{
			_open = true;
		}

		public override void OnNext(TSource value)
		{
			if (_open)
			{
				ForwardOnNext(value);
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly DateTimeOffset _startTime;

	internal readonly IScheduler _scheduler;

	public SkipUntil(IObservable<TSource> source, DateTimeOffset startTime, IScheduler scheduler)
	{
		_source = source;
		_startTime = startTime;
		_scheduler = scheduler;
	}

	public IObservable<TSource> Combine(DateTimeOffset startTime)
	{
		if (startTime <= _startTime)
		{
			return this;
		}
		return new SkipUntil<TSource>(_source, startTime, _scheduler);
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

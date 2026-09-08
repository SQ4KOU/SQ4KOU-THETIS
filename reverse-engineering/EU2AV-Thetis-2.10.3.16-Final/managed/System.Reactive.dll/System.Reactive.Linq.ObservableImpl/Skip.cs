using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Skip<TSource>
{
	internal sealed class Count : Producer<TSource, Count._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private int _remaining;

			public _(int count, IObserver<TSource> observer)
				: base(observer)
			{
				_remaining = count;
			}

			public override void OnNext(TSource value)
			{
				if (_remaining <= 0)
				{
					ForwardOnNext(value);
				}
				else
				{
					_remaining--;
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly int _count;

		public Count(IObservable<TSource> source, int count)
		{
			_source = source;
			_count = count;
		}

		public IObservable<TSource> Combine(int count)
		{
			return new Count(_source, _count + count);
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_count, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class Time : Producer<TSource, Time._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private volatile bool _open;

			private SingleAssignmentDisposableValue _sourceDisposable;

			public _(IObserver<TSource> observer)
				: base(observer)
			{
			}

			public void Run(Time parent)
			{
				SetUpstream(parent._scheduler.ScheduleAction(this, parent._duration, delegate(@_ state)
				{
					state.Tick();
				}));
				_sourceDisposable.Disposable = parent._source.SubscribeSafe(this);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_sourceDisposable.Dispose();
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

		private readonly TimeSpan _duration;

		internal readonly IScheduler _scheduler;

		public Time(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
		{
			_source = source;
			_duration = duration;
			_scheduler = scheduler;
		}

		public IObservable<TSource> Combine(TimeSpan duration)
		{
			if (duration <= _duration)
			{
				return this;
			}
			return new Time(_source, duration, _scheduler);
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

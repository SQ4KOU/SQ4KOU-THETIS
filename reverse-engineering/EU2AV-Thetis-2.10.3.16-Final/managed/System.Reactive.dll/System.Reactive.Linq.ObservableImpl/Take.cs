using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Take<TSource>
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
				if (_remaining > 0)
				{
					_remaining--;
					ForwardOnNext(value);
					if (_remaining == 0)
					{
						ForwardOnCompleted();
					}
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
			if (_count <= count)
			{
				return this;
			}
			return new Count(_source, count);
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
		internal sealed class @_(IObserver<TSource> observer) : IdentitySink<TSource>(observer)
		{
			private readonly object _gate = new object();

			private SingleAssignmentDisposableValue _task;

			public void Run(Time parent)
			{
				_task.Disposable = parent._scheduler.ScheduleAction(this, parent._duration, delegate(@_ state)
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
				lock (_gate)
				{
					ForwardOnCompleted();
				}
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					ForwardOnNext(value);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					ForwardOnCompleted();
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
			if (_duration <= duration)
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

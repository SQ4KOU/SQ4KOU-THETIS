using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl;

internal static class SkipLast<TSource>
{
	internal sealed class Count : Producer<TSource, Count._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly int _count;

			private readonly Queue<TSource> _queue;

			public _(int count, IObserver<TSource> observer)
				: base(observer)
			{
				_count = count;
				_queue = new Queue<TSource>();
			}

			public override void OnNext(TSource value)
			{
				_queue.Enqueue(value);
				if (_queue.Count > _count)
				{
					ForwardOnNext(_queue.Dequeue());
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
			private readonly TimeSpan _duration;

			private readonly Queue<System.Reactive.TimeInterval<TSource>> _queue;

			private IStopwatch? _watch;

			public _(TimeSpan duration, IObserver<TSource> observer)
				: base(observer)
			{
				_duration = duration;
				_queue = new Queue<System.Reactive.TimeInterval<TSource>>();
			}

			public void Run(Time parent)
			{
				_watch = parent._scheduler.StartStopwatch();
				SetUpstream(parent._source.SubscribeSafe(this));
			}

			public override void OnNext(TSource value)
			{
				TimeSpan elapsed = _watch.Elapsed;
				_queue.Enqueue(new System.Reactive.TimeInterval<TSource>(value, elapsed));
				while (_queue.Count > 0 && elapsed - _queue.Peek().Interval >= _duration)
				{
					ForwardOnNext(_queue.Dequeue().Value);
				}
			}

			public override void OnCompleted()
			{
				TimeSpan elapsed = _watch.Elapsed;
				while (_queue.Count > 0 && elapsed - _queue.Peek().Interval >= _duration)
				{
					ForwardOnNext(_queue.Dequeue().Value);
				}
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly TimeSpan _duration;

		private readonly IScheduler _scheduler;

		public Time(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
		{
			_source = source;
			_duration = duration;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_duration, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}
}

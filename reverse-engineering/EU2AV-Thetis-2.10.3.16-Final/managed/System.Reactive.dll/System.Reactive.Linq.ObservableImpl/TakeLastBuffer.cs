using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl;

internal static class TakeLastBuffer<TSource>
{
	internal sealed class Count : Producer<IList<TSource>, Count._>
	{
		internal sealed class @_ : Sink<TSource, IList<TSource>>
		{
			private readonly int _count;

			private readonly Queue<TSource> _queue;

			public _(int count, IObserver<IList<TSource>> observer)
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
					_queue.Dequeue();
				}
			}

			public override void OnCompleted()
			{
				List<TSource> list = new List<TSource>(_queue.Count);
				while (_queue.Count > 0)
				{
					list.Add(_queue.Dequeue());
				}
				ForwardOnNext(list);
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly int _count;

		public Count(IObservable<TSource> source, int count)
		{
			_source = source;
			_count = count;
		}

		protected override @_ CreateSink(IObserver<IList<TSource>> observer)
		{
			return new @_(_count, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class Time : Producer<IList<TSource>, Time._>
	{
		internal sealed class @_ : Sink<TSource, IList<TSource>>
		{
			private readonly TimeSpan _duration;

			private readonly Queue<System.Reactive.TimeInterval<TSource>> _queue;

			private IStopwatch? _watch;

			public _(TimeSpan duration, IObserver<IList<TSource>> observer)
				: base(observer)
			{
				_duration = duration;
				_queue = new Queue<System.Reactive.TimeInterval<TSource>>();
			}

			public void Run(IObservable<TSource> source, IScheduler scheduler)
			{
				_watch = scheduler.StartStopwatch();
				Run(source);
			}

			public override void OnNext(TSource value)
			{
				TimeSpan elapsed = _watch.Elapsed;
				_queue.Enqueue(new System.Reactive.TimeInterval<TSource>(value, elapsed));
				Trim(elapsed);
			}

			public override void OnCompleted()
			{
				TimeSpan elapsed = _watch.Elapsed;
				Trim(elapsed);
				List<TSource> list = new List<TSource>(_queue.Count);
				while (_queue.Count > 0)
				{
					list.Add(_queue.Dequeue().Value);
				}
				ForwardOnNext(list);
				ForwardOnCompleted();
			}

			private void Trim(TimeSpan now)
			{
				while (_queue.Count > 0 && now - _queue.Peek().Interval >= _duration)
				{
					_queue.Dequeue();
				}
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

		protected override @_ CreateSink(IObserver<IList<TSource>> observer)
		{
			return new @_(_duration, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source, _scheduler);
		}
	}
}

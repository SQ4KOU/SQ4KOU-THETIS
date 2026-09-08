using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class TimeInterval<TSource> : Producer<System.Reactive.TimeInterval<TSource>, TimeInterval<TSource>._>
{
	internal sealed class @_ : Sink<TSource, System.Reactive.TimeInterval<TSource>>
	{
		private IStopwatch? _watch;

		private TimeSpan _last;

		public _(IObserver<System.Reactive.TimeInterval<TSource>> observer)
			: base(observer)
		{
		}

		public void Run(TimeInterval<TSource> parent)
		{
			_watch = parent._scheduler.StartStopwatch();
			_last = TimeSpan.Zero;
			SetUpstream(parent._source.Subscribe(this));
		}

		public override void OnNext(TSource value)
		{
			TimeSpan elapsed = _watch.Elapsed;
			TimeSpan interval = elapsed.Subtract(_last);
			_last = elapsed;
			ForwardOnNext(new System.Reactive.TimeInterval<TSource>(value, interval));
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly IScheduler _scheduler;

	public TimeInterval(IObservable<TSource> source, IScheduler scheduler)
	{
		_source = source;
		_scheduler = scheduler;
	}

	protected override @_ CreateSink(IObserver<System.Reactive.TimeInterval<TSource>> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(this);
	}
}

using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl;

internal abstract class DelaySubscription<TSource> : Producer<TSource, DelaySubscription<TSource>._>
{
	internal sealed class Relative : DelaySubscription<TSource>
	{
		private readonly TimeSpan _dueTime;

		public Relative(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
			: base(source, scheduler)
		{
			_dueTime = dueTime;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new DelaySubscription<TSource>._(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source, _scheduler, _dueTime);
		}
	}

	internal sealed class Absolute : DelaySubscription<TSource>
	{
		private readonly DateTimeOffset _dueTime;

		public Absolute(IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
			: base(source, scheduler)
		{
			_dueTime = dueTime;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new DelaySubscription<TSource>._(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source, _scheduler, _dueTime);
		}
	}

	internal sealed class @_ : IdentitySink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(IObservable<TSource> source, IScheduler scheduler, DateTimeOffset dueTime)
		{
			SetUpstream(scheduler.ScheduleAction((this, source), dueTime, ((@_ @this, IObservable<TSource> source) tuple) => tuple.source.SubscribeSafe(tuple.@this)));
		}

		public void Run(IObservable<TSource> source, IScheduler scheduler, TimeSpan dueTime)
		{
			SetUpstream(scheduler.ScheduleAction((this, source), dueTime, ((@_ @this, IObservable<TSource> source) tuple) => tuple.source.SubscribeSafe(tuple.@this)));
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly IScheduler _scheduler;

	protected DelaySubscription(IObservable<TSource> source, IScheduler scheduler)
	{
		_source = source;
		_scheduler = scheduler;
	}
}

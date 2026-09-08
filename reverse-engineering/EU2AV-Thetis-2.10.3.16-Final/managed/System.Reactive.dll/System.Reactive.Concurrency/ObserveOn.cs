using System.Threading;

namespace System.Reactive.Concurrency;

internal static class ObserveOn<TSource>
{
	internal sealed class Scheduler : Producer<TSource, ObserveOnObserverNew<TSource>>
	{
		private readonly IObservable<TSource> _source;

		private readonly IScheduler _scheduler;

		public Scheduler(IObservable<TSource> source, IScheduler scheduler)
		{
			_source = source;
			_scheduler = scheduler;
		}

		protected override ObserveOnObserverNew<TSource> CreateSink(IObserver<TSource> observer)
		{
			return new ObserveOnObserverNew<TSource>(_scheduler, observer);
		}

		protected override void Run(ObserveOnObserverNew<TSource> sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class SchedulerLongRunning : Producer<TSource, ObserveOnObserverLongRunning<TSource>>
	{
		private readonly IObservable<TSource> _source;

		private readonly ISchedulerLongRunning _scheduler;

		public SchedulerLongRunning(IObservable<TSource> source, ISchedulerLongRunning scheduler)
		{
			_source = source;
			_scheduler = scheduler;
		}

		protected override ObserveOnObserverLongRunning<TSource> CreateSink(IObserver<TSource> observer)
		{
			return new ObserveOnObserverLongRunning<TSource>(_scheduler, observer);
		}

		protected override void Run(ObserveOnObserverLongRunning<TSource> sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class Context : Producer<TSource, Context._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly SynchronizationContext _context;

			public _(SynchronizationContext context, IObserver<TSource> observer)
				: base(observer)
			{
				_context = context;
			}

			public override void Run(IObservable<TSource> source)
			{
				_context.OperationStarted();
				SetUpstream(source.SubscribeSafe(this));
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_context.OperationCompleted();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TSource value)
			{
				_context.Post(OnNextPosted, value);
			}

			public override void OnError(Exception error)
			{
				_context.Post(OnErrorPosted, error);
			}

			public override void OnCompleted()
			{
				_context.Post(OnCompletedPosted, null);
			}

			private void OnNextPosted(object? value)
			{
				ForwardOnNext((TSource)value);
			}

			private void OnErrorPosted(object? error)
			{
				ForwardOnError((Exception)error);
			}

			private void OnCompletedPosted(object? ignored)
			{
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly SynchronizationContext _context;

		public Context(IObservable<TSource> source, SynchronizationContext context)
		{
			_source = source;
			_context = context;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_context, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}

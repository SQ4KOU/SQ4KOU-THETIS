using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Concurrency;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class Synchronization
{
	private sealed class SubscribeOnObservable<TSource> : ObservableBase<TSource>
	{
		private sealed class Subscription : IDisposable
		{
			private SerialDisposableValue _cancel;

			public Subscription(IObservable<TSource> source, IScheduler scheduler, IObserver<TSource> observer)
			{
				_cancel.TrySetFirst(scheduler.Schedule((this, source, observer), delegate(IScheduler closureScheduler, (Subscription @this, IObservable<TSource> source, IObserver<TSource> observer) state)
				{
					state.@this._cancel.Disposable = new ScheduledDisposable(closureScheduler, state.source.SubscribeSafe(state.observer));
					return Disposable.Empty;
				}));
			}

			public void Dispose()
			{
				_cancel.Dispose();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly IScheduler _scheduler;

		public SubscribeOnObservable(IObservable<TSource> source, IScheduler scheduler)
		{
			_source = source;
			_scheduler = scheduler;
		}

		protected override IDisposable SubscribeCore(IObserver<TSource> observer)
		{
			return new Subscription(_source, _scheduler, observer);
		}
	}

	private sealed class SubscribeOnCtxObservable<TSource> : ObservableBase<TSource>
	{
		private sealed class Subscription : IDisposable
		{
			private readonly IObservable<TSource> _source;

			private readonly IObserver<TSource> _observer;

			private readonly SynchronizationContext _context;

			private SingleAssignmentDisposableValue _cancel;

			public Subscription(IObservable<TSource> source, SynchronizationContext context, IObserver<TSource> observer)
			{
				_source = source;
				_context = context;
				_observer = observer;
				context.PostWithStartComplete(delegate(Subscription @this)
				{
					if (!@this._cancel.IsDisposed)
					{
						@this._cancel.Disposable = new ContextDisposable(@this._context, @this._source.SubscribeSafe(@this._observer));
					}
				}, this);
			}

			public void Dispose()
			{
				_cancel.Dispose();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly SynchronizationContext _context;

		public SubscribeOnCtxObservable(IObservable<TSource> source, SynchronizationContext context)
		{
			_source = source;
			_context = context;
		}

		protected override IDisposable SubscribeCore(IObserver<TSource> observer)
		{
			return new Subscription(_source, _context, observer);
		}
	}

	public static IObservable<TSource> SubscribeOn<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new SubscribeOnObservable<TSource>(source, scheduler);
	}

	public static IObservable<TSource> SubscribeOn<TSource>(IObservable<TSource> source, SynchronizationContext context)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return new SubscribeOnCtxObservable<TSource>(source, context);
	}

	public static IObservable<TSource> ObserveOn<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		ISchedulerLongRunning schedulerLongRunning = scheduler.AsLongRunning();
		if (schedulerLongRunning != null)
		{
			return new ObserveOn<TSource>.SchedulerLongRunning(source, schedulerLongRunning);
		}
		return new ObserveOn<TSource>.Scheduler(source, scheduler);
	}

	public static IObservable<TSource> ObserveOn<TSource>(IObservable<TSource> source, SynchronizationContext context)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return new ObserveOn<TSource>.Context(source, context);
	}

	public static IObservable<TSource> Synchronize<TSource>(IObservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new Synchronize<TSource>(source);
	}

	public static IObservable<TSource> Synchronize<TSource>(IObservable<TSource> source, object gate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (gate == null)
		{
			throw new ArgumentNullException("gate");
		}
		return new Synchronize<TSource>(source, gate);
	}
}

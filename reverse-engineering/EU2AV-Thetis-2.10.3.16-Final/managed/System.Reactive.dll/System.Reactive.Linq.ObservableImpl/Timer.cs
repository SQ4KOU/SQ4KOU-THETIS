using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Timer
{
	internal abstract class Single : Producer<long, Single._>
	{
		internal sealed class Relative : Single
		{
			private readonly TimeSpan _dueTime;

			public Relative(TimeSpan dueTime, IScheduler scheduler)
				: base(scheduler)
			{
				_dueTime = dueTime;
			}

			protected override @_ CreateSink(IObserver<long> observer)
			{
				return new @_(observer);
			}

			protected override void Run(@_ sink)
			{
				sink.Run(this, _dueTime);
			}
		}

		internal sealed class Absolute : Single
		{
			private readonly DateTimeOffset _dueTime;

			public Absolute(DateTimeOffset dueTime, IScheduler scheduler)
				: base(scheduler)
			{
				_dueTime = dueTime;
			}

			protected override @_ CreateSink(IObserver<long> observer)
			{
				return new @_(observer);
			}

			protected override void Run(@_ sink)
			{
				sink.Run(this, _dueTime);
			}
		}

		internal sealed class @_ : IdentitySink<long>
		{
			public _(IObserver<long> observer)
				: base(observer)
			{
			}

			public void Run(Single parent, DateTimeOffset dueTime)
			{
				SetUpstream(parent._scheduler.ScheduleAction(this, dueTime, delegate(@_ state)
				{
					state.Invoke();
				}));
			}

			public void Run(Single parent, TimeSpan dueTime)
			{
				SetUpstream(parent._scheduler.ScheduleAction(this, dueTime, delegate(@_ state)
				{
					state.Invoke();
				}));
			}

			private void Invoke()
			{
				ForwardOnNext(0L);
				ForwardOnCompleted();
			}
		}

		private readonly IScheduler _scheduler;

		protected Single(IScheduler scheduler)
		{
			_scheduler = scheduler;
		}
	}

	internal abstract class Periodic : Producer<long, Periodic._>
	{
		internal sealed class Relative : Periodic
		{
			private readonly TimeSpan _dueTime;

			public Relative(TimeSpan dueTime, TimeSpan period, IScheduler scheduler)
				: base(period, scheduler)
			{
				_dueTime = dueTime;
			}

			protected override @_ CreateSink(IObserver<long> observer)
			{
				return new @_(_period, observer);
			}

			protected override void Run(@_ sink)
			{
				sink.Run(this, _dueTime);
			}
		}

		internal sealed class Absolute : Periodic
		{
			private readonly DateTimeOffset _dueTime;

			public Absolute(DateTimeOffset dueTime, TimeSpan period, IScheduler scheduler)
				: base(period, scheduler)
			{
				_dueTime = dueTime;
			}

			protected override @_ CreateSink(IObserver<long> observer)
			{
				return new @_(_period, observer);
			}

			protected override void Run(@_ sink)
			{
				sink.Run(this, _dueTime);
			}
		}

		internal sealed class @_ : IdentitySink<long>
		{
			private readonly TimeSpan _period;

			private long _index;

			private int _pendingTickCount;

			private IDisposable? _periodic;

			public _(TimeSpan period, IObserver<long> observer)
				: base(observer)
			{
				_period = period;
			}

			public void Run(Periodic parent, DateTimeOffset dueTime)
			{
				SetUpstream(parent._scheduler.Schedule(this, dueTime, (IScheduler innerScheduler, @_ @this) => @this.InvokeStart(innerScheduler)));
			}

			public void Run(Periodic parent, TimeSpan dueTime)
			{
				if (dueTime == _period)
				{
					SetUpstream(parent._scheduler.SchedulePeriodic(this, _period, delegate(@_ @this)
					{
						@this.Tick();
					}));
				}
				else
				{
					SetUpstream(parent._scheduler.Schedule(this, dueTime, (IScheduler innerScheduler, @_ @this) => @this.InvokeStart(innerScheduler)));
				}
			}

			private void Tick()
			{
				ForwardOnNext(_index++);
			}

			private IDisposable InvokeStart(IScheduler self)
			{
				_pendingTickCount = 1;
				SingleAssignmentDisposable singleAssignmentDisposable = (SingleAssignmentDisposable)(_periodic = new SingleAssignmentDisposable());
				_index = 1L;
				singleAssignmentDisposable.Disposable = self.SchedulePeriodic(this, _period, delegate(@_ @this)
				{
					@this.Tock();
				});
				try
				{
					ForwardOnNext(0L);
				}
				catch (Exception exception)
				{
					singleAssignmentDisposable.Dispose();
					exception.Throw();
				}
				if (Interlocked.Decrement(ref _pendingTickCount) > 0)
				{
					IDisposable disposable = self.Schedule((this, 1L), delegate((@_ @this, long index) tuple, Action<(@_ @this, long index)> action)
					{
						tuple.@this.CatchUp(tuple.index, action);
					});
					return StableCompositeDisposable.Create(singleAssignmentDisposable, disposable);
				}
				return singleAssignmentDisposable;
			}

			private void Tock()
			{
				if (Interlocked.Increment(ref _pendingTickCount) == 1)
				{
					ForwardOnNext(_index++);
					Interlocked.Decrement(ref _pendingTickCount);
				}
			}

			private void CatchUp(long count, Action<(@_, long)> recurse)
			{
				try
				{
					ForwardOnNext(count);
				}
				catch (Exception exception)
				{
					_periodic.Dispose();
					exception.Throw();
				}
				if (Interlocked.Decrement(ref _pendingTickCount) > 0)
				{
					recurse((this, count + 1));
				}
			}
		}

		private readonly TimeSpan _period;

		private readonly IScheduler _scheduler;

		protected Periodic(TimeSpan period, IScheduler scheduler)
		{
			_period = period;
			_scheduler = scheduler;
		}
	}
}

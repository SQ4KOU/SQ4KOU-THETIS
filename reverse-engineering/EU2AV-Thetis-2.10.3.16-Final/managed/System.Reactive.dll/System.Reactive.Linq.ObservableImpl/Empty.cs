using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Empty<TResult> : Producer<TResult, Empty<TResult>._>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		public _(IObserver<TResult> observer)
			: base(observer)
		{
		}

		public void Run(IScheduler scheduler)
		{
			SetUpstream(scheduler.ScheduleAction(this, delegate(@_ target)
			{
				target.OnCompleted();
			}));
		}
	}

	private readonly IScheduler _scheduler;

	public Empty(IScheduler scheduler)
	{
		_scheduler = scheduler;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_scheduler);
	}
}

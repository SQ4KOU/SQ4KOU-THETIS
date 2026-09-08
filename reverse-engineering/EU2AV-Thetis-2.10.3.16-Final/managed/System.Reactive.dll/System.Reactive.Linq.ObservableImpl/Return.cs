using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Return<TResult> : Producer<TResult, Return<TResult>._>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		private readonly TResult _value;

		public _(TResult value, IObserver<TResult> observer)
			: base(observer)
		{
			_value = value;
		}

		public void Run(IScheduler scheduler)
		{
			SetUpstream(scheduler.ScheduleAction(this, delegate(@_ @this)
			{
				@this.Invoke();
			}));
		}

		private void Invoke()
		{
			ForwardOnNext(_value);
			ForwardOnCompleted();
		}
	}

	private readonly TResult _value;

	private readonly IScheduler _scheduler;

	public Return(TResult value, IScheduler scheduler)
	{
		_value = value;
		_scheduler = scheduler;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_value, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_scheduler);
	}
}

using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class RangeLongRunning : Producer<int, RangeLongRunning.RangeSink>
{
	internal sealed class RangeSink : IdentitySink<int>
	{
		private readonly int _end;

		private readonly int _index;

		public RangeSink(int start, int count, IObserver<int> observer)
			: base(observer)
		{
			_index = start;
			_end = start + count;
		}

		public void Run(ISchedulerLongRunning scheduler)
		{
			SetUpstream(scheduler.ScheduleLongRunning(this, delegate(RangeSink @this, ICancelable cancel)
			{
				@this.Loop(cancel);
			}));
		}

		private void Loop(ICancelable cancel)
		{
			int index = _index;
			int end = _end;
			while (!cancel.IsDisposed && index != end)
			{
				ForwardOnNext(index++);
			}
			if (!cancel.IsDisposed)
			{
				ForwardOnCompleted();
			}
		}
	}

	private readonly int _start;

	private readonly int _count;

	private readonly ISchedulerLongRunning _scheduler;

	public RangeLongRunning(int start, int count, ISchedulerLongRunning scheduler)
	{
		_start = start;
		_count = count;
		_scheduler = scheduler;
	}

	protected override RangeSink CreateSink(IObserver<int> observer)
	{
		return new RangeSink(_start, _count, observer);
	}

	protected override void Run(RangeSink sink)
	{
		sink.Run(_scheduler);
	}
}

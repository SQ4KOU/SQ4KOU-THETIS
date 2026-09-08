using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class RangeRecursive : Producer<int, RangeRecursive.RangeSink>
{
	internal sealed class RangeSink : IdentitySink<int>
	{
		private readonly int _end;

		private int _index;

		private MultipleAssignmentDisposableValue _task;

		public RangeSink(int start, int count, IObserver<int> observer)
			: base(observer)
		{
			_index = start;
			_end = start + count;
		}

		public void Run(IScheduler scheduler)
		{
			IDisposable disposable = scheduler.Schedule(this, (IScheduler innerScheduler, RangeSink @this) => @this.LoopRec(innerScheduler));
			_task.TrySetFirst(disposable);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_task.Dispose();
			}
		}

		private IDisposable LoopRec(IScheduler scheduler)
		{
			int index = _index;
			if (index != _end)
			{
				_index = index + 1;
				ForwardOnNext(index);
				IDisposable disposable = scheduler.Schedule(this, (IScheduler innerScheduler, RangeSink @this) => @this.LoopRec(innerScheduler));
				_task.Disposable = disposable;
			}
			else
			{
				ForwardOnCompleted();
			}
			return Disposable.Empty;
		}
	}

	private readonly int _start;

	private readonly int _count;

	private readonly IScheduler _scheduler;

	public RangeRecursive(int start, int count, IScheduler scheduler)
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

using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Repeat<TResult>
{
	internal sealed class ForeverRecursive : Producer<TResult, ForeverRecursive._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private readonly TResult _value;

			private MultipleAssignmentDisposableValue _task;

			public _(TResult value, IObserver<TResult> observer)
				: base(observer)
			{
				_value = value;
			}

			public void Run(IScheduler scheduler)
			{
				IDisposable disposable = scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.LoopRecInf(innerScheduler));
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

			private IDisposable LoopRecInf(IScheduler scheduler)
			{
				ForwardOnNext(_value);
				IDisposable disposable = scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.LoopRecInf(innerScheduler));
				_task.Disposable = disposable;
				return Disposable.Empty;
			}
		}

		private readonly TResult _value;

		private readonly IScheduler _scheduler;

		public ForeverRecursive(TResult value, IScheduler scheduler)
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

	internal sealed class ForeverLongRunning : Producer<TResult, ForeverLongRunning._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private readonly TResult _value;

			public _(TResult value, IObserver<TResult> observer)
				: base(observer)
			{
				_value = value;
			}

			public void Run(ISchedulerLongRunning longRunning)
			{
				SetUpstream(longRunning.ScheduleLongRunning(this, delegate(@_ @this, ICancelable c)
				{
					@this.LoopInf(c);
				}));
			}

			private void LoopInf(ICancelable cancel)
			{
				TResult value = _value;
				while (!cancel.IsDisposed)
				{
					ForwardOnNext(value);
				}
				Dispose();
			}
		}

		private readonly TResult _value;

		private readonly ISchedulerLongRunning _scheduler;

		public ForeverLongRunning(TResult value, ISchedulerLongRunning scheduler)
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

	internal sealed class CountRecursive : Producer<TResult, CountRecursive._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private readonly TResult _value;

			private int _remaining;

			private MultipleAssignmentDisposableValue _task;

			public _(TResult value, int repeatCount, IObserver<TResult> observer)
				: base(observer)
			{
				_value = value;
				_remaining = repeatCount;
			}

			public void Run(IScheduler scheduler)
			{
				IDisposable disposable = scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.LoopRec(innerScheduler));
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
				int num = _remaining;
				if (num > 0)
				{
					ForwardOnNext(_value);
					num = (_remaining = num - 1);
				}
				if (num == 0)
				{
					ForwardOnCompleted();
				}
				else
				{
					IDisposable disposable = scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.LoopRec(innerScheduler));
					_task.Disposable = disposable;
				}
				return Disposable.Empty;
			}
		}

		private readonly TResult _value;

		private readonly IScheduler _scheduler;

		private readonly int _repeatCount;

		public CountRecursive(TResult value, int repeatCount, IScheduler scheduler)
		{
			_value = value;
			_scheduler = scheduler;
			_repeatCount = repeatCount;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(_value, _repeatCount, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_scheduler);
		}
	}

	internal sealed class CountLongRunning : Producer<TResult, CountLongRunning._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private readonly TResult _value;

			private readonly int _remaining;

			public _(TResult value, int remaining, IObserver<TResult> observer)
				: base(observer)
			{
				_value = value;
				_remaining = remaining;
			}

			public void Run(ISchedulerLongRunning longRunning)
			{
				SetUpstream(longRunning.ScheduleLongRunning(this, delegate(@_ @this, ICancelable cancel)
				{
					@this.Loop(cancel);
				}));
			}

			private void Loop(ICancelable cancel)
			{
				TResult value = _value;
				int num = _remaining;
				while (num > 0 && !cancel.IsDisposed)
				{
					ForwardOnNext(value);
					num--;
				}
				if (!cancel.IsDisposed)
				{
					ForwardOnCompleted();
				}
			}
		}

		private readonly TResult _value;

		private readonly ISchedulerLongRunning _scheduler;

		private readonly int _repeatCount;

		public CountLongRunning(TResult value, int repeatCount, ISchedulerLongRunning scheduler)
		{
			_value = value;
			_scheduler = scheduler;
			_repeatCount = repeatCount;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(_value, _repeatCount, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_scheduler);
		}
	}
}

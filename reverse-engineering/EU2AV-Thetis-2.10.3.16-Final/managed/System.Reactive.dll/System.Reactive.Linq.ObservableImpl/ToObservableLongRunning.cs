using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ToObservableLongRunning<TSource> : Producer<TSource, ToObservableLongRunning<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(IEnumerable<TSource> source, ISchedulerLongRunning scheduler)
		{
			IEnumerator<TSource> enumerator;
			try
			{
				enumerator = source.GetEnumerator();
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			SetUpstream(scheduler.ScheduleLongRunning((this, enumerator), delegate((@_ @this, IEnumerator<TSource> e) tuple, ICancelable cancelable)
			{
				tuple.@this.Loop(tuple.e, cancelable);
			}));
		}

		private void Loop(IEnumerator<TSource> enumerator, ICancelable cancel)
		{
			while (!cancel.IsDisposed)
			{
				bool flag = false;
				Exception ex = null;
				TSource value = default(TSource);
				try
				{
					flag = enumerator.MoveNext();
					if (flag)
					{
						value = enumerator.Current;
					}
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
				if (ex != null)
				{
					ForwardOnError(ex);
					break;
				}
				if (!flag)
				{
					ForwardOnCompleted();
					break;
				}
				ForwardOnNext(value);
			}
			enumerator.Dispose();
			Dispose();
		}
	}

	private readonly IEnumerable<TSource> _source;

	private readonly ISchedulerLongRunning _scheduler;

	public ToObservableLongRunning(IEnumerable<TSource> source, ISchedulerLongRunning scheduler)
	{
		_source = source;
		_scheduler = scheduler;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source, _scheduler);
	}
}

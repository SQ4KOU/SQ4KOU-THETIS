using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ToObservableRecursive<TSource> : Producer<TSource, ToObservableRecursive<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private IEnumerator<TSource>? _enumerator;

		private volatile bool _disposed;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(IEnumerable<TSource> source, IScheduler scheduler)
		{
			try
			{
				_enumerator = source.GetEnumerator();
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.LoopRec(innerScheduler));
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_disposed = true;
			}
		}

		private IDisposable LoopRec(IScheduler scheduler)
		{
			bool flag = false;
			Exception ex = null;
			TSource value = default(TSource);
			IEnumerator<TSource> enumerator = _enumerator;
			if (_disposed)
			{
				enumerator.Dispose();
				_enumerator = null;
				return Disposable.Empty;
			}
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
				enumerator.Dispose();
				_enumerator = null;
				ForwardOnError(ex);
				return Disposable.Empty;
			}
			if (!flag)
			{
				enumerator.Dispose();
				_enumerator = null;
				ForwardOnCompleted();
				return Disposable.Empty;
			}
			ForwardOnNext(value);
			scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.LoopRec(innerScheduler));
			return Disposable.Empty;
		}
	}

	private readonly IEnumerable<TSource> _source;

	private readonly IScheduler _scheduler;

	public ToObservableRecursive(IEnumerable<TSource> source, IScheduler scheduler)
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

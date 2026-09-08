using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Finally<TSource> : Producer<TSource, Finally<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly Action _finallyAction;

		private IDisposable? _sourceDisposable;

		public _(Action finallyAction, IObserver<TSource> observer)
			: base(observer)
		{
			_finallyAction = finallyAction;
		}

		public override void Run(IObservable<TSource> source)
		{
			IDisposable disposable = source.SubscribeSafe(this);
			if (Interlocked.CompareExchange(ref _sourceDisposable, disposable, null) == BooleanDisposable.True)
			{
				try
				{
					disposable.Dispose();
				}
				finally
				{
					_finallyAction();
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing)
			{
				return;
			}
			IDisposable disposable = Interlocked.Exchange(ref _sourceDisposable, BooleanDisposable.True);
			if (disposable != BooleanDisposable.True && disposable != null)
			{
				try
				{
					disposable.Dispose();
				}
				finally
				{
					_finallyAction();
				}
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Action _finallyAction;

	public Finally(IObservable<TSource> source, Action finallyAction)
	{
		_source = source;
		_finallyAction = finallyAction;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(_finallyAction, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

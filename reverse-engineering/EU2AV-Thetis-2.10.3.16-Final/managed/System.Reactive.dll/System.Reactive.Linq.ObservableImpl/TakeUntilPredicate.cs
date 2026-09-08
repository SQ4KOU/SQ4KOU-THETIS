namespace System.Reactive.Linq.ObservableImpl;

internal sealed class TakeUntilPredicate<TSource> : Producer<TSource, TakeUntilPredicate<TSource>.TakeUntilPredicateObserver>
{
	internal sealed class TakeUntilPredicateObserver : IdentitySink<TSource>
	{
		private readonly Func<TSource, bool> _stopPredicate;

		public TakeUntilPredicateObserver(IObserver<TSource> downstream, Func<TSource, bool> predicate)
			: base(downstream)
		{
			_stopPredicate = predicate;
		}

		public override void OnCompleted()
		{
			ForwardOnCompleted();
		}

		public override void OnError(Exception error)
		{
			ForwardOnError(error);
		}

		public override void OnNext(TSource value)
		{
			ForwardOnNext(value);
			bool flag = false;
			try
			{
				flag = _stopPredicate(value);
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			if (flag)
			{
				ForwardOnCompleted();
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, bool> _stopPredicate;

	public TakeUntilPredicate(IObservable<TSource> source, Func<TSource, bool> stopPredicate)
	{
		_source = source;
		_stopPredicate = stopPredicate;
	}

	protected override TakeUntilPredicateObserver CreateSink(IObserver<TSource> observer)
	{
		return new TakeUntilPredicateObserver(observer, _stopPredicate);
	}

	protected override void Run(TakeUntilPredicateObserver sink)
	{
		sink.Run(_source);
	}
}

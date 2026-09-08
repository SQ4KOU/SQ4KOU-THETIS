namespace System.Reactive.Linq.ObservableImpl;

internal sealed class All<TSource> : Producer<bool, All<TSource>._>
{
	internal sealed class @_ : Sink<TSource, bool>
	{
		private readonly Func<TSource, bool> _predicate;

		public _(Func<TSource, bool> predicate, IObserver<bool> observer)
			: base(observer)
		{
			_predicate = predicate;
		}

		public override void OnNext(TSource value)
		{
			bool flag;
			try
			{
				flag = _predicate(value);
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			if (!flag)
			{
				ForwardOnNext(value: false);
				ForwardOnCompleted();
			}
		}

		public override void OnCompleted()
		{
			ForwardOnNext(value: true);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, bool> _predicate;

	public All(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		_source = source;
		_predicate = predicate;
	}

	protected override @_ CreateSink(IObserver<bool> observer)
	{
		return new @_(_predicate, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

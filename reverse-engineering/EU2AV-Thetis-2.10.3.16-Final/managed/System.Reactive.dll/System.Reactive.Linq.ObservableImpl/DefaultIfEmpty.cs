namespace System.Reactive.Linq.ObservableImpl;

internal sealed class DefaultIfEmpty<TSource> : Producer<TSource, DefaultIfEmpty<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly TSource _defaultValue;

		private bool _found;

		public _(TSource defaultValue, IObserver<TSource> observer)
			: base(observer)
		{
			_defaultValue = defaultValue;
			_found = false;
		}

		public override void OnNext(TSource value)
		{
			_found = true;
			ForwardOnNext(value);
		}

		public override void OnCompleted()
		{
			if (!_found)
			{
				ForwardOnNext(_defaultValue);
			}
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly TSource _defaultValue;

	public DefaultIfEmpty(IObservable<TSource> source, TSource defaultValue)
	{
		_source = source;
		_defaultValue = defaultValue;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(_defaultValue, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

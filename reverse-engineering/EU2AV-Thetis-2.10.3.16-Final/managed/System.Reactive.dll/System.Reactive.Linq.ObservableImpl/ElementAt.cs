namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ElementAt<TSource> : Producer<TSource, ElementAt<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private int _i;

		public _(int index, IObserver<TSource> observer)
			: base(observer)
		{
			_i = index;
		}

		public override void OnNext(TSource value)
		{
			if (_i == 0)
			{
				ForwardOnNext(value);
				ForwardOnCompleted();
			}
			_i--;
		}

		public override void OnCompleted()
		{
			if (_i >= 0)
			{
				try
				{
					throw new ArgumentOutOfRangeException("index");
				}
				catch (Exception error)
				{
					ForwardOnError(error);
				}
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly int _index;

	public ElementAt(IObservable<TSource> source, int index)
	{
		_source = source;
		_index = index;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(_index, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

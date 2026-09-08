namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SumDecimalNullable : Producer<decimal?, SumDecimalNullable._>
{
	internal sealed class @_ : IdentitySink<decimal?>
	{
		private decimal _sum;

		public _(IObserver<decimal?> observer)
			: base(observer)
		{
		}

		public override void OnNext(decimal? value)
		{
			if (value.HasValue)
			{
				_sum += value.Value;
			}
		}

		public override void OnCompleted()
		{
			ForwardOnNext(_sum);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<decimal?> _source;

	public SumDecimalNullable(IObservable<decimal?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<decimal?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

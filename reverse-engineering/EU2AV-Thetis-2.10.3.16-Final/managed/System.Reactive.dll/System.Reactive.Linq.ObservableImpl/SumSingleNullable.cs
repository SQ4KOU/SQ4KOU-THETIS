namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SumSingleNullable : Producer<float?, SumSingleNullable._>
{
	internal sealed class @_ : IdentitySink<float?>
	{
		private double _sum;

		public _(IObserver<float?> observer)
			: base(observer)
		{
		}

		public override void OnNext(float? value)
		{
			if (value.HasValue)
			{
				_sum += value.Value;
			}
		}

		public override void OnCompleted()
		{
			ForwardOnNext((float)_sum);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<float?> _source;

	public SumSingleNullable(IObservable<float?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<float?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

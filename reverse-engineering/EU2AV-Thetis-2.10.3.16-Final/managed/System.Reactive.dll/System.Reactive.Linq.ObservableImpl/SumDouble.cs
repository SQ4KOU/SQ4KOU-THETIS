namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SumDouble : Producer<double, SumDouble._>
{
	internal sealed class @_ : IdentitySink<double>
	{
		private double _sum;

		public _(IObserver<double> observer)
			: base(observer)
		{
		}

		public override void OnNext(double value)
		{
			_sum += value;
		}

		public override void OnCompleted()
		{
			ForwardOnNext(_sum);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<double> _source;

	public SumDouble(IObservable<double> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<double> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

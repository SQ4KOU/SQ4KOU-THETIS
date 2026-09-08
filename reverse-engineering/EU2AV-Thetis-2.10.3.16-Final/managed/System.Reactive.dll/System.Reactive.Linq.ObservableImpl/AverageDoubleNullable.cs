namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageDoubleNullable : Producer<double?, AverageDoubleNullable._>
{
	internal sealed class @_ : IdentitySink<double?>
	{
		private double _sum;

		private long _count;

		public _(IObserver<double?> observer)
			: base(observer)
		{
			_sum = 0.0;
			_count = 0L;
		}

		public override void OnNext(double? value)
		{
			checked
			{
				try
				{
					if (value.HasValue)
					{
						_sum += value.Value;
						_count++;
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
				}
			}
		}

		public override void OnCompleted()
		{
			if (_count > 0)
			{
				ForwardOnNext(_sum / (double)_count);
			}
			else
			{
				ForwardOnNext(null);
			}
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<double?> _source;

	public AverageDoubleNullable(IObservable<double?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<double?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

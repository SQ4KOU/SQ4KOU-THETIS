namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageSingleNullable : Producer<float?, AverageSingleNullable._>
{
	internal sealed class @_ : IdentitySink<float?>
	{
		private double _sum;

		private long _count;

		public _(IObserver<float?> observer)
			: base(observer)
		{
			_sum = 0.0;
			_count = 0L;
		}

		public override void OnNext(float? value)
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
				ForwardOnNext((float)(_sum / (double)_count));
			}
			else
			{
				ForwardOnNext(null);
			}
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<float?> _source;

	public AverageSingleNullable(IObservable<float?> source)
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

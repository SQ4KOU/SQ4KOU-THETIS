namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageDecimalNullable : Producer<decimal?, AverageDecimalNullable._>
{
	internal sealed class @_ : IdentitySink<decimal?>
	{
		private decimal _sum;

		private long _count;

		public _(IObserver<decimal?> observer)
			: base(observer)
		{
			_sum = 0m;
			_count = 0L;
		}

		public override void OnNext(decimal? value)
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
				ForwardOnNext(_sum / (decimal)_count);
			}
			else
			{
				ForwardOnNext(null);
			}
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<decimal?> _source;

	public AverageDecimalNullable(IObservable<decimal?> source)
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

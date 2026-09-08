namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageInt64Nullable : Producer<double?, AverageInt64Nullable._>
{
	internal sealed class @_ : Sink<long?, double?>
	{
		private long _sum;

		private long _count;

		public _(IObserver<double?> observer)
			: base(observer)
		{
			_sum = 0L;
			_count = 0L;
		}

		public override void OnNext(long? value)
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
				ForwardOnNext((double)_sum / (double)_count);
			}
			else
			{
				ForwardOnNext(null);
			}
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<long?> _source;

	public AverageInt64Nullable(IObservable<long?> source)
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

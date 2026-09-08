namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SumInt64Nullable : Producer<long?, SumInt64Nullable._>
{
	internal sealed class @_ : IdentitySink<long?>
	{
		private long _sum;

		public _(IObserver<long?> observer)
			: base(observer)
		{
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
			ForwardOnNext(_sum);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<long?> _source;

	public SumInt64Nullable(IObservable<long?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<long?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

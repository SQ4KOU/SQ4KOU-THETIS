namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SumInt32Nullable : Producer<int?, SumInt32Nullable._>
{
	internal sealed class @_ : IdentitySink<int?>
	{
		private int _sum;

		public _(IObserver<int?> observer)
			: base(observer)
		{
		}

		public override void OnNext(int? value)
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

	private readonly IObservable<int?> _source;

	public SumInt32Nullable(IObservable<int?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<int?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

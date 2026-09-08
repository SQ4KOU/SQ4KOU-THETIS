namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SumInt32 : Producer<int, SumInt32._>
{
	internal sealed class @_ : IdentitySink<int>
	{
		private int _sum;

		public _(IObserver<int> observer)
			: base(observer)
		{
		}

		public override void OnNext(int value)
		{
			checked
			{
				try
				{
					_sum += value;
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

	private readonly IObservable<int> _source;

	public SumInt32(IObservable<int> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<int> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

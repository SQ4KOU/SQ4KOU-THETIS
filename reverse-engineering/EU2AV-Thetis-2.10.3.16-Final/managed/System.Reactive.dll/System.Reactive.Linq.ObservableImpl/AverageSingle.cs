namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageSingle : Producer<float, AverageSingle._>
{
	internal sealed class @_ : IdentitySink<float>
	{
		private double _sum;

		private long _count;

		public _(IObserver<float> observer)
			: base(observer)
		{
			_sum = 0.0;
			_count = 0L;
		}

		public override void OnNext(float value)
		{
			checked
			{
				try
				{
					_sum += value;
					_count++;
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
				ForwardOnCompleted();
				return;
			}
			try
			{
				throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
			}
			catch (Exception error)
			{
				ForwardOnError(error);
			}
		}
	}

	private readonly IObservable<float> _source;

	public AverageSingle(IObservable<float> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<float> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

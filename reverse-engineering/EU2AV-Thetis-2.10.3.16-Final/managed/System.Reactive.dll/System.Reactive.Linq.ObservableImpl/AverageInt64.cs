namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageInt64 : Producer<double, AverageInt64._>
{
	internal sealed class @_ : Sink<long, double>
	{
		private long _sum;

		private long _count;

		public _(IObserver<double> observer)
			: base(observer)
		{
			_sum = 0L;
			_count = 0L;
		}

		public override void OnNext(long value)
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
				ForwardOnNext((double)_sum / (double)_count);
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

	private readonly IObservable<long> _source;

	public AverageInt64(IObservable<long> source)
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

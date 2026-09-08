namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageDouble : Producer<double, AverageDouble._>
{
	internal sealed class @_ : IdentitySink<double>
	{
		private double _sum;

		private long _count;

		public _(IObserver<double> observer)
			: base(observer)
		{
			_sum = 0.0;
			_count = 0L;
		}

		public override void OnNext(double value)
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
				ForwardOnNext(_sum / (double)_count);
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

	private readonly IObservable<double> _source;

	public AverageDouble(IObservable<double> source)
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

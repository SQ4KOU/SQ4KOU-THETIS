namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AverageDecimal : Producer<decimal, AverageDecimal._>
{
	internal sealed class @_ : IdentitySink<decimal>
	{
		private decimal _sum;

		private long _count;

		public _(IObserver<decimal> observer)
			: base(observer)
		{
			_sum = 0m;
			_count = 0L;
		}

		public override void OnNext(decimal value)
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
				ForwardOnNext(_sum / (decimal)_count);
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

	private readonly IObservable<decimal> _source;

	public AverageDecimal(IObservable<decimal> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<decimal> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}

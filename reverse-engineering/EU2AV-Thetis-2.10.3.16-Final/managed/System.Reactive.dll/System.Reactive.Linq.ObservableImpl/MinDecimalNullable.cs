namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MinDecimalNullable : Producer<decimal?, MinDecimalNullable._>
{
	internal sealed class @_ : IdentitySink<decimal?>
	{
		private decimal? _lastValue;

		public _(IObserver<decimal?> observer)
			: base(observer)
		{
		}

		public override void OnNext(decimal? value)
		{
			if (!value.HasValue)
			{
				return;
			}
			if (_lastValue.HasValue)
			{
				if (value < _lastValue)
				{
					_lastValue = value;
				}
			}
			else
			{
				_lastValue = value;
			}
		}

		public override void OnCompleted()
		{
			ForwardOnNext(_lastValue);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<decimal?> _source;

	public MinDecimalNullable(IObservable<decimal?> source)
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

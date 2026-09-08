namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MaxDoubleNullable : Producer<double?, MaxDoubleNullable._>
{
	internal sealed class @_ : IdentitySink<double?>
	{
		private double? _lastValue;

		public _(IObserver<double?> observer)
			: base(observer)
		{
		}

		public override void OnNext(double? value)
		{
			if (!value.HasValue)
			{
				return;
			}
			if (_lastValue.HasValue)
			{
				if (value > _lastValue || double.IsNaN(value.Value))
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

	private readonly IObservable<double?> _source;

	public MaxDoubleNullable(IObservable<double?> source)
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

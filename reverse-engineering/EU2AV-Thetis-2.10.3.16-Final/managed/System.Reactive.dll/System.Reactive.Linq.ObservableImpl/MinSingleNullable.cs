namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MinSingleNullable : Producer<float?, MinSingleNullable._>
{
	internal sealed class @_ : IdentitySink<float?>
	{
		private float? _lastValue;

		public _(IObserver<float?> observer)
			: base(observer)
		{
		}

		public override void OnNext(float? value)
		{
			if (!value.HasValue)
			{
				return;
			}
			if (_lastValue.HasValue)
			{
				if (value < _lastValue || float.IsNaN(value.Value))
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

	private readonly IObservable<float?> _source;

	public MinSingleNullable(IObservable<float?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<float?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
